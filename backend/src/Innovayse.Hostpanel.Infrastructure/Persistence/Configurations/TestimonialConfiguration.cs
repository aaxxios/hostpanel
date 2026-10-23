using Innovayse.Hostpanel.Domain.Testimonials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="Testimonial"/>.</summary>
public sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(Testimonial.NameMaxLength);
        builder.Property(x => x.Company).HasMaxLength(Testimonial.CompanyMaxLength);
        builder.Property(x => x.AvatarUrl).HasMaxLength(Testimonial.AvatarUrlMaxLength);
        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.PublishedAt);
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.TestimonialId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
