namespace TrackManagement.Application.Features.Artists;

public sealed record CreateArtistRequest(string Name, string Email, string Country);

public sealed record ArtistResponse(Guid Id, string Name, string Email, string Country);