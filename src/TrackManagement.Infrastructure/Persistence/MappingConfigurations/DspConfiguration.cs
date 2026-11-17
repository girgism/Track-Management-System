using TrackManagement.Domain.Entities.DigitalServiceProviders;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TrackManagement.Infrastructure.Persistence.MappingConfigurations;

public sealed class DspConfiguration : IEntityTypeConfiguration<DSP>
{
    public void Configure(EntityTypeBuilder<DSP> builder)
    {
        builder.ToTable("DSPs");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(DSP.NameMaxLength);

        builder.HasIndex(d => d.Name)
            .IsUnique()
            .HasDatabaseName("UX_Dsps_Name");
    }
}
