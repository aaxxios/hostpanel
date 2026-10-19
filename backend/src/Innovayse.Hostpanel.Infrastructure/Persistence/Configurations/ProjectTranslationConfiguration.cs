using Innovayse.Hostpanel.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="ProjectTranslation"/>.</summary>
public sealed class ProjectTranslationConfiguration : IEntityTypeConfiguration<ProjectTranslation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ProjectTranslation> builder)
    {
        builder.ToTable("project_translations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.Locale).IsRequired().HasMaxLength(ProjectTranslation.LocaleMaxLength);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(ProjectTranslation.TitleMaxLength);
        builder.Property(x => x.Summary).HasMaxLength(ProjectTranslation.SummaryMaxLength);
        builder.Property(x => x.Description).HasMaxLength(ProjectTranslation.DescriptionMaxLength);
        builder.HasIndex(x => new { x.ProjectId, x.Locale }).IsUnique();
    }
}
