namespace TrackManagement.Application.Features.DSPs;

public interface IDspRepository
{
    /// <summary>Returns the subset of the given ids that exist.</summary>
    Task<IReadOnlyCollection<Guid>> GetExistingIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}