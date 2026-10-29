using Innovayse.Hostpanel.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table configuration for <see cref="JobRun"/>.</summary>
public sealed class JobRunConfiguration : IEntityTypeConfiguration<JobRun>
{
    /// <summary>Configures the <c>JobRuns</c> table mapping.</summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<JobRun> builder)
    {
        builder.HasKey(x => x.JobName);

        builder.Property(x => x.JobName).HasMaxLength(200);
        builder.Property(x => x.LeaseOwner).HasMaxLength(100);

        // Two instances that read the same free row and both take the lease cannot both save it.
        builder.UseXminConcurrencyToken();
    }
}
