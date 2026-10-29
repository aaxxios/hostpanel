using Innovayse.Hostpanel.Domain.Faqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="Faq"/>.</summary>
public sealed class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.HasKey(x => x.Id);

        // Two operators reordering the same list at once cannot overwrite each other: the second save of a
        // changed row fails with a concurrency conflict (409) instead of silently winning.
        builder.UseXminConcurrencyToken();

        builder.Property(x => x.Category).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.FaqId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
