using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TrackManagement.Application.Common.Models;
using TrackManagement.Application.Features.Artists;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/artists")]
[Produces("application/json")]
public sealed class ArtistsController(CreateArtistUseCase createArtistUseCase,
                                      ListArtistsUseCase listArtistsUseCase) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CommandResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateArtistRequest request,
                                            CancellationToken cancellationToken)
    {
        var response = await createArtistUseCase.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArtistResponse>>(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var artists = await listArtistsUseCase.ExecuteAsync(cancellationToken);
        return Ok(artists);
    }
}