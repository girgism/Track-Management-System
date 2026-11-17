using TrackManagement.Domain.Entities.Tracks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TrackManagement.Infrastructure.Persistence.MappingConfigurations;

public sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("Tracks", table =>
        {
            table.HasCheckConstraint("CK_Tracks_Status", EnumSql.InList<TrackStatus>(nameof(Track.Status)));
            table.HasCheckConstraint("CK_Tracks_Genre", EnumSql.InList<Genre>(nameof(Track.Genre)));
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(Track.TitleMaxLength);

        // Value object <-> canonical 12-char string. Reading re-validates through Isrc.Create.
        builder.Property(t => t.Isrc)
            .HasConversion(isrc => isrc.Value, value => Isrc.Create(value))
            .IsRequired()
            .HasMaxLength(Isrc.Length)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(t => t.ReleaseDate).IsRequired();

        builder.Property(t => t.Genre)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        // Optimistic concurrency token that lives only in the database/EF model, not on the Domain class.
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(t => t.Isrc)
            .IsUnique()
            .HasDatabaseName("UX_Tracks_Isrc");

        builder.HasIndex(t => t.ArtistId).HasDatabaseName("IX_Tracks_ArtistId");
        builder.HasIndex(t => t.Status).HasDatabaseName("IX_Tracks_Status");

        builder.HasOne(t => t.Artist)
            .WithMany()
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Distributions)
            .WithOne(d => d.Track)
            .HasForeignKey(d => d.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        // Distributions is a read-only view over the private _distributions list.
        builder.Navigation(t => t.Distributions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
