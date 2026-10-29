using Innovayse.Hostpanel.Domain.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table configuration for <see cref="MigrationLog"/>.</summary>
public sealed class MigrationLogConfiguration : IEntityTypeConfiguration<MigrationLog>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<MigrationLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.JobId).IsRequired();
        builder.Property(x => x.EntityType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Identifier).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Action).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.SkipReason).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();

        // The (JobId, Action) index serves a job-only lookup through its JobId prefix.
        builder.HasIndex(x => new { x.JobId, x.Action });

        // A job's import report is deleted with the job.
        builder.HasOne<MigrationJob>()
            .WithMany()
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
