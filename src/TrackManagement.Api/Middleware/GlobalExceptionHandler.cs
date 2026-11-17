using System.Text.Json;

using FluentValidation;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Domain.Common;

namespace TrackManagement.Api.Middleware;

internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService,
                                             ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                Exception exception,
                                                CancellationToken cancellationToken)
    {
        // The client went away. Not an error, and nobody is left to read a body.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled by the client: {Method} {Path}",
                                  httpContext.Request.Method,
                                  httpContext.Request.Path);

            httpContext.Response.StatusCode = 499;
            return true;
        }

        var problem = ToProblemDetails(exception);
        var status = problem.Status ?? StatusCodes.Status500InternalServerError;

        if (status >= 500)
        {
            logger.LogError(exception,
                            "Unhandled exception while processing {Method} {Path}",
                            httpContext.Request.Method,
                            httpContext.Request.Path);
        }
        else
        {
            // Code and status only: exception messages can contain user data such as emails.
            logger.LogWarning("Request rejected with {Status} ({Code}) on {Method} {Path}",
                              status,
                              problem.Extensions["code"],
                              httpContext.Request.Method,
                              httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails ToProblemDetails(Exception exception) => exception switch
    {
        ValidationException ex => ValidationProblem(
            ex.Errors
                .GroupBy(e => CamelCase(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray()),
            "validation.failed",
            "One or more validation errors occurred."),

        DomainValidationException ex => ValidationProblem(
            new Dictionary<string, string[]> { [CamelCase(ex.Field)] = [ex.Message] },
            ex.Code,
            "One or more validation errors occurred."),

        InvalidReferenceException ex => ValidationProblem(
            new Dictionary<string, string[]> { [CamelCase(ex.Field)] = [ex.Message] },
            ex.Code,
            "A referenced resource does not exist."),

        NotFoundException ex => Problem(StatusCodes.Status404NotFound, "Resource not found", ex.Code, ex.Message),

        ConflictException ex => Problem(StatusCodes.Status409Conflict, "Conflict", ex.Code, ex.Message),

        DomainException ex => Problem(StatusCodes.Status409Conflict, "Business rule violation", ex.Code, ex.Message),

        AuthenticationFailedException ex => Problem(StatusCodes.Status401Unauthorized, "Authentication failed", ex.Code, ex.Message),

        _ => Problem(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.",
            "server.error",
            "The server could not process the request. Quote the traceId when reporting this problem.")
    };

    private static HttpValidationProblemDetails ValidationProblem(IDictionary<string, string[]> errors,
                                                                  string code,
                                                                  string title)
    {
        var problem = new HttpValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title
        };
        problem.Extensions["code"] = code;
        return problem;
    }

    private static ProblemDetails Problem(int status, string title, string code, string detail)
    {
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        problem.Extensions["code"] = code;
        return problem;
    }

    private static string CamelCase(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
}