namespace TrackManagement.Application.Common.Exceptions;

/// <summary>Base for expected, client-facing failures raised by use cases. Code is a stable machine-readable id.</summary>
public abstract class AppException : Exception
{
    protected AppException(string code, string message, Exception? inner = null) : base(message, inner)
    {
        Code = code;
    }

    public string Code { get; }
}

/// <summary>The resource addressed by the request URL does not exist (404).</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, Guid id)
        : base($"{resource.ToLowerInvariant()}.not_found", $"{resource} '{id}' was not found.")
    {
    }
}

/// <summary>A field in the request body references something that does not exist (422).</summary>
public sealed class InvalidReferenceException : AppException
{
    public InvalidReferenceException(string field, string message)
        : base("reference.not_found", message)
    {
        Field = field;
    }

    public string Field { get; }
}

/// <summary>The request conflicts with the current state: duplicates or concurrent modification (409).</summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string code, string message, Exception? inner = null)
        : base(code, message, inner)
    {
    }
}

/// <summary>Credentials were wrong. Deliberately vague so it never reveals which part failed (401).</summary>
public sealed class AuthenticationFailedException : AppException
{
    public AuthenticationFailedException()
        : base("auth.invalid_credentials", "Invalid email or password.")
    {
    }
}