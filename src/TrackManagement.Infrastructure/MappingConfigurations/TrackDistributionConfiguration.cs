using global::TrackManagement.Domain.Entities.Distributions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TrackManagement.Infrastructure.MappingConfigurations;

public sealed class TrackDistributionConfiguration : IEntityTypeConfiguration<TrackDistribution>
{
    public void Configure(EntityTypeBuilder<TrackDistribution> builder)
    {
        builder.ToTable("TrackDistributions", table =>
            table.HasCheckConstraint(
                "CK_TrackDistributions_Status",
                EnumSql.InList<DistributionStatus>(nameof(TrackDistribution.Status))));

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.SubmittedAt).IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // One distribution per (track, DSP). This is the authoritative guard against concurrent duplicates.
        builder.HasIndex(d => new { d.TrackId, d.DspId })
            .IsUnique()
            .HasDatabaseName("UX_TrackDistributions_TrackId_DspId");

        builder.HasIndex(d => d.DspId).HasDatabaseName("IX_TrackDistributions_DspId");

        builder.HasOne(d => d.Dsp)
            .WithMany()
            .HasForeignKey(d => d.DspId)
            .OnDelete(DeleteBehavior.Restrict);

        // The Track -> Distributions relationship (and its cascade) is configured in TrackConfiguration.
    }
}
