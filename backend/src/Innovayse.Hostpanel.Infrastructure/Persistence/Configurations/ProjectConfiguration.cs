using Innovayse.Hostpanel.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="Project"/>.</summary>
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.Id);

        // Two operators reordering the same list at once cannot overwrite each other: the second save of a
        // changed row fails with a concurrency conflict (409) instead of silently winning.
        builder.UseXminConcurrencyToken();

        builder.Property(x => x.Slug).IsRequired().HasMaxLength(Project.SlugMaxLength);
        builder.Property(x => x.Url).HasMaxLength(Project.UrlMaxLength);
        builder.Property(x => x.ImageUrls).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.SortOrder);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
