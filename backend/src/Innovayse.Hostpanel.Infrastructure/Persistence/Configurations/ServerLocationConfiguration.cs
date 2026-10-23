using Innovayse.Hostpanel.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="ServerLocation"/>.</summary>
public sealed class ServerLocationConfiguration : IEntityTypeConfiguration<ServerLocation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ServerLocation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CountryCode).IsRequired().HasMaxLength(ServerLocation.CountryCodeLength);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.ServerLocationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
