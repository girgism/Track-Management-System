using global::TrackManagement.Domain.Entities.Artists;
using global::TrackManagement.Domain.Entities.DigitalServiceProviders;
using global::TrackManagement.Domain.Entities.Distributions;
using global::TrackManagement.Domain.Entities.Tracks;

using Microsoft.EntityFrameworkCore;

namespace TrackManagement.Infrastructure.Seeding;

internal static class DbSeeder
{
    private static readonly DateTimeOffset SeedTime = new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);

    public static void Seed(DbContext context, bool storeOperationPerformed)
    {
        var db = (TrackManagemenContext)context;

        if (db.Artists.Any())
            return;

        Populate(db);
        db.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, bool storeOperationPerformed, CancellationToken cancellationToken)
    {
        var db = (TrackManagemenContext)context;

        if (await db.Artists.AnyAsync(cancellationToken))
            return;

        Populate(db);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void Populate(TrackManagemenContext db)
    {
        var nour = Artist.Create("Nour El-Sayed", "nour.elsayed@example.com", "EG");
        var layla = Artist.Create("Layla Haddad", "layla.haddad@example.com", "LB");
        var omar = Artist.Create("Omar Al-Rashid", "omar.alrashid@example.com", "SA");
        var karim = Artist.Create("Karim Fawzy", "karim.fawzy@example.com", "EG");

        var spotify = DSP.Create("Spotify");
        var apple = DSP.Create("Apple Music");
        var youtube = DSP.Create("YouTube");

        const DistributionStatus live = DistributionStatus.Live;
        const DistributionStatus pending = DistributionStatus.Pending;
        const DistributionStatus rejected = DistributionStatus.Rejected;

        var tracks = new[]
        {
            // Distributed
            BuildTrack(nour, "Sahar El Leil", "EG-N01-24-00001", new DateOnly(2024, 3, 15), Genre.Pop,
                TrackStatus.Distributed, 0, (spotify, live), (apple, live), (youtube, live)),
            BuildTrack(nour, "Nile Sunrise", "EG-N01-24-00002", new DateOnly(2024, 9, 1), Genre.Electronic,
                TrackStatus.Distributed, 1, (spotify, live), (youtube, live)),
            BuildTrack(layla, "Cairo After Midnight", "LB-L01-23-00001", new DateOnly(2023, 11, 20), Genre.Jazz,
                TrackStatus.Distributed, 2, (spotify, live), (apple, live)),

            // Submitted (some waiting, one partially rejected, one not yet sent anywhere)
            BuildTrack(nour, "Habibti Ya Donya", "EG-N01-25-00001", new DateOnly(2025, 2, 14), Genre.Pop,
                TrackStatus.Submitted, 3, (spotify, pending), (apple, pending)),
            BuildTrack(layla, "Cedars in the Rain", "LB-L01-25-00001", new DateOnly(2025, 5, 10), Genre.Folk,
                TrackStatus.Submitted, 4, (spotify, live), (apple, rejected)),
            BuildTrack(omar, "Desert Pulse", "SA-O01-25-00001", new DateOnly(2025, 8, 22), Genre.Electronic,
                TrackStatus.Submitted, 5, (youtube, pending)),
            BuildTrack(karim, "Zahma", "EG-K01-25-00001", new DateOnly(2025, 12, 5), Genre.Mahraganat,
                TrackStatus.Submitted, 6, (spotify, live), (youtube, rejected)),
            BuildTrack(omar, "Midnight Velvet", "SA-O01-26-00002", new DateOnly(2026, 4, 10), Genre.RnB,
                TrackStatus.Submitted, 7),

            // Draft
            BuildTrack(layla, "Beirut Lullaby", "LB-L01-26-00001", new DateOnly(2026, 1, 30), Genre.Classical,
                TrackStatus.Draft, 0),
            BuildTrack(omar, "Riyadh Skyline", "SA-O01-26-00001", new DateOnly(2026, 3, 1), Genre.HipHop,
                TrackStatus.Draft, 0),
            BuildTrack(karim, "Fe El Share3", "EG-K01-26-00001", new DateOnly(2026, 6, 20), Genre.Shaabi,
                TrackStatus.Draft, 0)
        };

        db.Artists.AddRange(nour, layla, omar, karim);
        db.Dsps.AddRange(spotify, apple, youtube);
        db.Tracks.AddRange(tracks); // distributions are reached through the Track aggregate
    }

    private static Track BuildTrack(Artist artist,
                                    string title,
                                    string isrc,
                                    DateOnly releaseDate,
                                    Genre genre,
                                    TrackStatus targetStatus,
                                    int submittedDaysAfterSeed,
                                    params (DSP Dsp, DistributionStatus Outcome)[] distributions)
    {
        var track = Track.Create(title, artist.Id, Isrc.Create(isrc), releaseDate, genre);

        if (targetStatus == TrackStatus.Draft)
            return track;

        track.ChangeStatus(TrackStatus.Submitted);

        if (distributions.Length > 0)
        {
            track.SubmitToDsps(
                distributions.Select(d => d.Dsp.Id).ToList(),
                SeedTime.AddDays(submittedDaysAfterSeed));

            foreach (var (dsp, outcome) in distributions)
            {
                switch (outcome)
                {
                    case DistributionStatus.Live:
                        track.MarkDistributionLive(dsp.Id);
                        break;
                    case DistributionStatus.Rejected:
                        track.RejectDistribution(dsp.Id);
                        break;
                }
            }
        }

        if (targetStatus == TrackStatus.Distributed)
            track.ChangeStatus(TrackStatus.Distributed);

        return track;
    }
}