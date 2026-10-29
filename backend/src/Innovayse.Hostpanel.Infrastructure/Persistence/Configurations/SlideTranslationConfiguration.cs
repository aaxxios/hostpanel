using System.Text.Encodings.Web;
using System.Text.Json;
using Innovayse.Hostpanel.Domain.Slides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table and column configuration for <see cref="SlideTranslation"/>.</summary>
public sealed class SlideTranslationConfiguration : IEntityTypeConfiguration<SlideTranslation>
{
    /// <summary>Writes the bullet points as JSON without turning Armenian and Russian letters into escapes.</summary>
    private static readonly JsonSerializerOptions FeatureJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SlideTranslation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SlideId).IsRequired();
        builder.Property(x => x.Locale).IsRequired().HasMaxLength(SlideTranslation.LocaleMaxLength);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(SlideTranslation.TitleMaxLength);
        builder.Property(x => x.Tagline).HasMaxLength(SlideTranslation.TaglineMaxLength);
        builder.Property(x => x.Description).HasMaxLength(SlideTranslation.DescriptionMaxLength);
        // The bullet points stay a JSON array in a text column, the shape the table has always
        // held, so the domain works with a string array while the stored rows keep their format.
        // The width bounds what the validator lets through (count times length, escaped).
        builder.Property(x => x.Features)
            .HasMaxLength(SlideTranslation.FeaturesColumnMaxLength)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, FeatureJson),
                v => v == null ? null : ReadFeatures(v),
                new ValueComparer<string[]?>(
                    (a, b) => a == null ? b == null : b != null && a.SequenceEqual(b),
                    v => v == null ? 0 : v.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                    v => v == null ? null : v.ToArray()));
        builder.Property(x => x.CtaText).HasMaxLength(SlideTranslation.CtaTextMaxLength);
        builder.Property(x => x.CtaUrl).HasMaxLength(SlideTranslation.CtaUrlMaxLength);
        builder.HasIndex(x => new { x.SlideId, x.Locale }).IsUnique();
    }

    /// <summary>
    /// Reads a stored bullet-point column back as an array. Every writer has stored a JSON array,
    /// but the column is free text and the readers before this converter treated anything that is
    /// not a JSON string array as "no bullet points" rather than failing the whole query. This keeps
    /// that contract in one place, so a hand-edited or otherwise malformed row degrades to no
    /// bullet points instead of breaking every slide list it appears in.
    /// </summary>
    /// <param name="stored">The column text, never null.</param>
    /// <returns>The bullet points, or null when the text is not a JSON array of strings.</returns>
    private static string[]? ReadFeatures(string stored)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(stored, FeatureJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
