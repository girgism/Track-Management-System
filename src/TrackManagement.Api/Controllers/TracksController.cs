using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TrackManagement.Application.Common.Models;
using TrackManagement.Application.Features.Tracks;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/tracks")]
[Produces("application/json")]
public sealed class TracksController(CreateTrackUseCase createTrackUseCase,
                                     ListTracksUseCase listTracksUseCase,
                                     GetTrackUseCase getTrackUseCase,
                                     DistributeTrackUseCase distributeTrackUseCase,
                                     ChangeTrackStatusUseCase changeTrackStatusUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTrackRequest request,
                                            CancellationToken cancellationToken)
    {
        var response = await createTrackUseCase.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TrackListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> ListAsync([FromQuery] TrackListQuery query,
                                               CancellationToken cancellationToken)
    {
        var page = await listTracksUseCase.ExecuteAsync(query, cancellationToken);
        return Ok(page.Items);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<TrackDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdAsync(Guid id,
                                                  CancellationToken cancellationToken)
    {
        var track = await getTrackUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(track);
    }


    [HttpPost("{id}/distribute")]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DistributeAsync(Guid id,
                                                [FromBody] DistributeTrackRequest request,
                                                [FromServices] DistributeTrackUseCase useCase,
                                                CancellationToken cancellationToken)
    {
        var response = await distributeTrackUseCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatusAsynce(Guid id,
                                                        [FromBody] ChangeTrackStatusRequest request,
                                                        CancellationToken cancellationToken)
    {
        var response = await changeTrackStatusUseCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(response);
    }
}