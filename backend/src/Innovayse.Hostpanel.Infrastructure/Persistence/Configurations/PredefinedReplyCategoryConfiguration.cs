using Innovayse.Hostpanel.Domain.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table configuration for <see cref="PredefinedReplyCategory"/>.</summary>
public sealed class PredefinedReplyCategoryConfiguration : IEntityTypeConfiguration<PredefinedReplyCategory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PredefinedReplyCategory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ParentCategoryId);

        // A category with sub-categories cannot be deleted; the delete use case refuses it first.
        // Named here because the default name runs past the 63 characters PostgreSQL keeps, and the
        // truncated form ends in a "~" that is not a PascalCase identifier.
        builder.HasOne<PredefinedReplyCategory>()
            .WithMany()
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PredefinedReplyCategories_ParentCategoryId");
    }
}
