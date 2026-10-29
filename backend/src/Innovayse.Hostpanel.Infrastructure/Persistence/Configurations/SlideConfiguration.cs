using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Slides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="Slide"/>.</summary>
public sealed class SlideConfiguration : IEntityTypeConfiguration<Slide>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Slide> builder)
    {
        builder.HasKey(x => x.Id);

        // Two operators reordering the same list at once cannot overwrite each other: the second save of a
        // changed row fails with a concurrency conflict (409) instead of silently winning.
        builder.UseXminConcurrencyToken();

        builder.Property(x => x.IconName).IsRequired().HasMaxLength(Slide.IconNameMaxLength);
        builder.Property(x => x.BrandColor).IsRequired().HasMaxLength(Slide.BrandColorMaxLength);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(Slide.ImageUrlMaxLength);
        builder.Property(x => x.DemoUrl).HasMaxLength(Slide.LinkMaxLength);
        builder.Property(x => x.LearnMoreUrl).HasMaxLength(Slide.LinkMaxLength);
        builder.Property(x => x.ProductId);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.TargetAudience)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(Slide.TargetAudienceMaxLength);
        builder.Property(x => x.VisibleFrom);
        builder.Property(x => x.VisibleUntil);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.SlideId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(x => x.DomainEvents);
        builder.Navigation(x => x.Translations).AutoInclude();

        // A slide outlives the product it promotes: deleting the product clears the link.
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
