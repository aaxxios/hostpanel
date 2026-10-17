using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Shared.Constants;
using Xunit;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Every error code has a sentence in every language, and every error sentence belongs to a code.
/// <c>IStringLocalizer</c> answers the key on a miss, so without this a customer would read
/// <c>INVOICE_NOT_FOUND</c> and nothing would fail.
/// </summary>
/// <remarks>
/// <c>ApplicationValidationMessages</c> (Application and Shared) joins the translated-keys theory in A4, when
/// the validators move to <c>LocalizedValidator&lt;T&gt;</c> and their untranslated keys are written.
/// </remarks>
public sealed class ResourceCoverageTests
{
    /// <summary>The codes the registry declares.</summary>
    private static readonly IReadOnlyList<string> Codes = typeof(HostpanelErrorCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral)
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToList();

    /// <summary>
    /// Each supported locale resolves each code to a non-empty sentence that is not the code itself;
    /// a translated locale must not silently fall back to the neutral English value.
    /// </summary>
    /// <param name="locale">A supported locale.</param>
    [Theory]
    [InlineData("en")]
    [InlineData("ru")]
    [InlineData("hy")]
    public void EveryCode_InSupportedLocale_HasASentence(string locale)
    {
        var manager = new ResourceManager(typeof(ErrorMessages));
        var culture = CultureInfo.GetCultureInfo(locale);
        var translated = locale != SupportedLanguages.Default;

        var missing = Codes.Where(code =>
        {
            var value = manager.GetString(code, culture);
            return string.IsNullOrWhiteSpace(value)
                   || value == code
                   || (translated && value == manager.GetString(code, CultureInfo.InvariantCulture));
        }).ToList();

        missing.Should().BeEmpty("ErrorMessages.{0}.resx lacks {1}", locale, string.Join(", ", missing));
    }

    /// <summary>No locale's own error resource file holds a key the registry does not declare.</summary>
    /// <param name="locale">A supported locale.</param>
    [Theory]
    [InlineData("en")]
    [InlineData("ru")]
    [InlineData("hy")]
    public void EverySentence_InSupportedLocale_BelongsToACode(string locale)
    {
        var keys = OwnEntries(typeof(ErrorMessages), locale).Keys;

        keys.Except(Codes).Should().BeEmpty("ErrorMessages resx for {0} has keys without a code", locale);
    }

    /// <summary>
    /// A translated error resource file carries exactly the neutral file's keys, each with its own
    /// non-empty value rather than a copy of the English one.
    /// </summary>
    /// <param name="locale">A translated locale.</param>
    [Theory]
    [InlineData("ru")]
    [InlineData("hy")]
    public void ErrorMessages_InTranslatedLocale_MatchNeutralKeysWithOwnValues(string locale)
    {
        var neutral = OwnEntries(typeof(ErrorMessages), SupportedLanguages.Default);
        var translated = OwnEntries(typeof(ErrorMessages), locale);

        translated.Keys.Should().BeEquivalentTo(
            neutral.Keys,
            "ErrorMessages.{0}.resx must hold the neutral keys",
            locale);

        var untranslated = translated
            .Where(entry => string.IsNullOrWhiteSpace(entry.Value)
                            || entry.Value == neutral.GetValueOrDefault(entry.Key))
            .Select(entry => entry.Key)
            .ToList();

        untranslated.Should().BeEmpty("ErrorMessages.{0}.resx has empty or English values", locale);
    }

    /// <summary>
    /// Every slide validation message the validators look up is in the neutral file and, in a
    /// translated locale, carries its own value rather than the English one. A miss would show the
    /// admin the bare key (or the library's English default) in a slide form.
    /// </summary>
    /// <param name="locale">A translated locale.</param>
    [Theory]
    [InlineData("ru")]
    [InlineData("hy")]
    public void SlideValidationMessages_InTranslatedLocale_HaveOwnValues(string locale)
    {
        var neutral = OwnEntries(typeof(ApplicationValidationMessages), SupportedLanguages.Default)
            .Where(entry => entry.Key.StartsWith("Slide", StringComparison.Ordinal))
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        var translated = OwnEntries(typeof(ApplicationValidationMessages), locale);

        neutral.Keys.Should().Contain(["SlideIconNameRequired", "SlideIconNameTooLong", "SlideImageUrlRequired", "SlideImageUrlTooLong", "SlideImageRequired"]);

        var untranslated = neutral.Keys
            .Where(key => !translated.TryGetValue(key, out var value)
                          || string.IsNullOrWhiteSpace(value)
                          || value == neutral[key])
            .ToList();

        untranslated.Should().BeEmpty("ApplicationValidationMessages.{0}.resx lacks its own slide sentences", locale);
    }

    /// <summary>
    /// The entries a locale's own resource file holds, without parent-culture fallback. The default
    /// locale is the neutral file.
    /// </summary>
    /// <param name="resourceType">The marker type naming the resource set.</param>
    /// <param name="locale">A supported locale.</param>
    /// <returns>Key to value, empty when the locale has no file of its own.</returns>
    private static Dictionary<string, string?> OwnEntries(Type resourceType, string locale)
    {
        var culture = locale == SupportedLanguages.Default
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(locale);
        var set = new ResourceManager(resourceType).GetResourceSet(culture, true, false);

        return set is null
            ? []
            : set.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => entry.Value as string);
    }
}
