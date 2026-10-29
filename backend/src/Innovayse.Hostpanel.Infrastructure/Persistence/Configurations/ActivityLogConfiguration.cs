using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="ActivityLog"/> entity.</summary>
public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    /// <summary>Configures the <see cref="ActivityLog"/> entity mapping.</summary>
    /// <param name="builder">Entity type builder.</param>
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ClientId);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(1000);
        builder.Property(l => l.AdminId).HasMaxLength(450);
        builder.Property(l => l.AdminName).HasMaxLength(200);
        builder.Property(l => l.AdminEmail).HasMaxLength(255);
        builder.Property(l => l.IpAddress).HasMaxLength(45);
        builder.Property(l => l.CreatedAt).IsRequired();

        // A client's activity is read newest first; the composite also serves the client-only lookup.
        builder.HasIndex(l => new { l.ClientId, l.CreatedAt });

        // The audit trail outlives the client: deleting the client keeps the entry and clears the link.
        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
