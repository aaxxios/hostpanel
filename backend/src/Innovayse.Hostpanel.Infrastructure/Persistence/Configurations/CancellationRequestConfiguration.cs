using Innovayse.Hostpanel.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="CancellationRequest"/>.</summary>
public sealed class CancellationRequestConfiguration : IEntityTypeConfiguration<CancellationRequest>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<CancellationRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ServiceId).IsRequired();
        builder.Property(x => x.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CreatedAt).IsRequired();

        // A cancellation request has no meaning without its service.
        builder.HasOne<ClientService>()
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
