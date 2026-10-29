using Innovayse.Hostpanel.Domain.Partners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="Partner"/>.</summary>
public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.HasKey(x => x.Id);

        // Two operators reordering the same list at once cannot overwrite each other: the second save of a
        // changed row fails with a concurrency conflict (409) instead of silently winning.
        builder.UseXminConcurrencyToken();

        builder.Property(x => x.LogoUrl).HasMaxLength(Partner.LogoUrlMaxLength);
        builder.Property(x => x.Url).HasMaxLength(Partner.UrlMaxLength);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
