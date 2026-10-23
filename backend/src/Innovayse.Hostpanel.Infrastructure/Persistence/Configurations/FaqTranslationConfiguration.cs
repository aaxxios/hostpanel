using Innovayse.Hostpanel.Domain.Faqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="FaqTranslation"/>.</summary>
public sealed class FaqTranslationConfiguration : IEntityTypeConfiguration<FaqTranslation>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FaqTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FaqId).IsRequired();
        builder.Property(x => x.Locale).IsRequired().HasMaxLength(FaqTranslation.LocaleMaxLength);
        builder.Property(x => x.Question).IsRequired().HasMaxLength(FaqTranslation.QuestionMaxLength);
        builder.Property(x => x.Answer).IsRequired().HasMaxLength(FaqTranslation.AnswerMaxLength);
        builder.HasIndex(x => new { x.FaqId, x.Locale }).IsUnique();
    }
}
