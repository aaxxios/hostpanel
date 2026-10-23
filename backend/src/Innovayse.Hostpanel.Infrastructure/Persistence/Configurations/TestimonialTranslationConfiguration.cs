using Innovayse.Hostpanel.Domain.Testimonials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="TestimonialTranslation"/>.</summary>
public sealed class TestimonialTranslationConfiguration : IEntityTypeConfiguration<TestimonialTranslation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TestimonialTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TestimonialId).IsRequired();
        builder.Property(x => x.Locale).IsRequired().HasMaxLength(TestimonialTranslation.LocaleMaxLength);
        builder.Property(x => x.Role).HasMaxLength(TestimonialTranslation.RoleMaxLength);
        builder.Property(x => x.Quote).IsRequired().HasMaxLength(TestimonialTranslation.QuoteMaxLength);
        builder.HasIndex(x => new { x.TestimonialId, x.Locale }).IsUnique();
    }
}
