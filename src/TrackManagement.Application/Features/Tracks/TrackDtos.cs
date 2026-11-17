using TrackManagement.Application.Features.Artists;
using TrackManagement.Domain.Entities.Distributions;
using TrackManagement.Domain.Entities.Tracks;

namespace TrackManagement.Application.Features.Tracks;

public sealed record CreateTrackRequest(string Title,
                                        Guid ArtistId,
                                        string Isrc,
                                        DateOnly ReleaseDate,
                                        Genre Genre);

public sealed record DistributeTrackRequest(IReadOnlyList<Guid> DspIds);

public sealed record ChangeTrackStatusRequest(TrackStatus Status);

/// <summary>Filters and paging for the track list. Bound from the query string.</summary>
public sealed record TrackListQuery(Guid? ArtistId,
                                    Genre? Genre,
                                    TrackStatus? Status,
                                    int Page = 1,
                                    int PageSize = 20);

public sealed record TrackListItemResponse(Guid Id,
                                           string Title,
                                           Guid ArtistId,
                                           string ArtistName,
                                           string Isrc,

                                           DateOnly ReleaseDate,
                                           Genre Genre,
                                           TrackStatus Status);

public sealed record TrackDistributionResponse(
    Guid Id,
    Guid DspId,
    string DspName,
    DateTimeOffset SubmittedAt,
    DistributionStatus Status);

public sealed record TrackDetailsResponse(
    Guid Id,
    string Title,
    ArtistResponse Artist,
    string Isrc,
    DateOnly ReleaseDate,
    Genre Genre,
    TrackStatus Status,
    IReadOnlyList<TrackDistributionResponse> Distributions);