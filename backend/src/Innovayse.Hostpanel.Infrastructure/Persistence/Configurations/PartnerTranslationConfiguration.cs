using Innovayse.Hostpanel.Domain.Partners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="PartnerTranslation"/>.</summary>
public sealed class PartnerTranslationConfiguration : IEntityTypeConfiguration<PartnerTranslation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PartnerTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PartnerId).IsRequired();
        builder.Property(x => x.Locale).IsRequired().HasMaxLength(PartnerTranslation.LocaleMaxLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(PartnerTranslation.NameMaxLength);
        builder.HasIndex(x => new { x.PartnerId, x.Locale }).IsUnique();
    }
}
