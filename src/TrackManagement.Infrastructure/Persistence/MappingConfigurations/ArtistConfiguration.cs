using TrackManagement.Domain.Entities.Artists;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TrackManagement.Infrastructure.Persistence.MappingConfigurations;

public sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("Artists");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(Artist.NameMaxLength);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(Artist.EmailMaxLength);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(Artist.CountryCodeLength)
            .IsFixedLength()
            .IsUnicode(false);

        builder.HasIndex(a => a.Email)
            .IsUnique()
            .HasDatabaseName("UX_Artists_Email");
    }
}
