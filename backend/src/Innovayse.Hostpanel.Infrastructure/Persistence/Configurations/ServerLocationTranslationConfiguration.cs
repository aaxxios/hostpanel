using Innovayse.Hostpanel.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="ServerLocationTranslation"/>.</summary>
public sealed class ServerLocationTranslationConfiguration : IEntityTypeConfiguration<ServerLocationTranslation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ServerLocationTranslation> builder)
    {
        builder.ToTable("server_location_translations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ServerLocationId).IsRequired();
        builder.Property(x => x.Locale).IsRequired().HasMaxLength(ServerLocationTranslation.LocaleMaxLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(ServerLocationTranslation.NameMaxLength);
        builder.Property(x => x.City).HasMaxLength(ServerLocationTranslation.CityMaxLength);
        builder.Property(x => x.LatencyNote).HasMaxLength(ServerLocationTranslation.LatencyNoteMaxLength);
        builder.HasIndex(x => new { x.ServerLocationId, x.Locale }).IsUnique();
    }
}
