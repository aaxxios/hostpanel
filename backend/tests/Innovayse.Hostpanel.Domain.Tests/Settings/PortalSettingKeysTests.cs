using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Domain.Settings.Enums;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Settings;

/// <summary>
/// Tests for <see cref="PortalSettingKeys"/>: the invariants that keep the seed list, the
/// public allow-list and the value vocabularies from drifting apart. Nothing else would notice
/// — a key seeded but not public just renders the build-time default, silently.
/// </summary>
/// <remarks>The type under test is a Domain policy class with no Application dependency.</remarks>
public class PortalSettingKeysTests
{
    /// <summary>Every seeded key is public and every public key is seeded.</summary>
    [Fact]
    public void Public_MatchesDefaults_Exactly()
    {
        var seeded = PortalSettingKeys.Defaults.Select(d => d.Key).ToHashSet();

        Assert.Equal(seeded, PortalSettingKeys.Public);
    }

    /// <summary>A seeded key appears once; a duplicate would make the seeder insert it twice.</summary>
    [Fact]
    public void Defaults_HaveNoDuplicateKeys()
    {
        var keys = PortalSettingKeys.Defaults.Select(d => d.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    /// <summary>Every key with a vocabulary is a seeded, public key.</summary>
    [Fact]
    public void AllowedValues_OnlyNameSeededKeys()
    {
        foreach (var key in PortalSettingKeys.AllowedValues.Keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
        }
    }

    /// <summary>
    /// The value a fresh install starts from must itself be writable: in the vocabulary, or
    /// empty for a key that allows empty.
    /// </summary>
    [Fact]
    public void Defaults_ForVocabularyKeys_AreAllowed()
    {
        foreach (var (key, value, _) in PortalSettingKeys.Defaults)
        {
            if (PortalSettingKeys.AllowedValues.TryGetValue(key, out var allowed))
            {
                Assert.True(
                    allowed.Contains(value) || (value.Length == 0 && PortalSettingKeys.AllowsEmpty(key)),
                    $"{key} seeds '{value}', which it would refuse");
            }
        }
    }

    /// <summary>Every shaped key is seeded, public, and seeds empty — the shape only applies to a set value.</summary>
    [Fact]
    public void Patterns_OnlyNameSeededKeysThatAllowEmpty()
    {
        foreach (var key in PortalSettingKeys.Patterns.Keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.True(PortalSettingKeys.AllowsEmpty(key), $"{key} must seed empty");
            Assert.False(
                PortalSettingKeys.AllowedValues.ContainsKey(key), $"{key} cannot have both a vocabulary and a shape");
        }
    }

    /// <summary>A key that seeds with a value never accepts an empty one.</summary>
    [Fact]
    public void AllowsEmpty_IsFalseForKeysThatSeedWithAValue()
    {
        Assert.False(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.Template));
        Assert.False(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.ThemeDefault));
        Assert.True(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.BrandPrimary));
        Assert.True(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.BrandFontBody));
        Assert.False(PortalSettingKeys.AllowsEmpty("integration:cwp7:hostname"));
    }

    /// <summary>A fresh install seeds the classic template, spelled as the storefront matches it.</summary>
    [Fact]
    public void Defaults_SeedTheClassicTemplate()
    {
        Assert.Equal(StorefrontTemplate.Classic, PortalSettingKeys.DefaultTemplate);
        Assert.Equal("classic", PortalSettingKeys.Defaults.Single(d => d.Key == PortalSettingKeys.Template).Value);
    }

    /// <summary>The template vocabulary matches what the storefront's registry can render.</summary>
    [Fact]
    public void TemplateNames_MatchTheClientRegistry()
    {
        Assert.Equal(
            new[] { "aurora", "classic", "nova" },
            PortalSettingKeys.TemplateNames.OrderBy(n => n).ToArray());
    }

    /// <summary>The hero figures and the office address and map are seeded empty and public.</summary>
    [Fact]
    public void HeroAndOfficeKeys_AreSeededEmptyAndPublic()
    {
        string[] keys =
        [
            PortalSettingKeys.StatsClients,
            PortalSettingKeys.StatsUptime,
            PortalSettingKeys.StatsRegions,
            PortalSettingKeys.HeroBadge,
            PortalSettingKeys.ContactAddress,
            PortalSettingKeys.ContactMapUrl,
        ];

        Assert.Equal(
            ["portal.stats.clients", "portal.stats.uptime", "portal.stats.regions", "portal.hero.badge", "portal.contact.address", "portal.contact.map_url"],
            keys);
        foreach (var key in keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.True(PortalSettingKeys.AllowsEmpty(key), $"{key} must seed empty so no figure is invented");
        }
    }

    /// <summary>The social, hours, About and nova switch keys are seeded empty and public; the switches accept only booleans.</summary>
    [Fact]
    public void StorefrontExtras_AreSeededEmptyAndPublic()
    {
        string[] switches =
        [
            PortalSettingKeys.FeaturesBackups,
            PortalSettingKeys.FeaturesStorage,
            PortalSettingKeys.FeaturesCaching,
            PortalSettingKeys.FeaturesCdn,
            PortalSettingKeys.FeaturesFirewall,
            PortalSettingKeys.FeaturesMalware,
            PortalSettingKeys.MigrationFree,
        ];
        string[] keys =
        [
            PortalSettingKeys.SocialTwitter,
            PortalSettingKeys.ContactHours,
            PortalSettingKeys.ContactTimezone,
            PortalSettingKeys.AboutText,
            PortalSettingKeys.AboutTextRu,
            PortalSettingKeys.AboutTextHy,
            .. switches,
        ];

        foreach (var key in keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.True(PortalSettingKeys.AllowsEmpty(key), $"{key} must seed empty so no claim is invented");
        }

        foreach (var key in switches)
        {
            Assert.Same(PortalSettingKeys.Booleans, PortalSettingKeys.AllowedValues[key]);
        }
    }

    /// <summary>The home-page section toggles seed "true" (projects "false"), are public, and accept only booleans, never empty.</summary>
    [Fact]
    public void SectionToggles_SeedVisible_ExceptProjects_AndAcceptOnlyBooleans()
    {
        string[] sections =
        [
            PortalSettingKeys.SectionStats,
            PortalSettingKeys.SectionPartners,
            PortalSettingKeys.SectionServices,
            PortalSettingKeys.SectionProcess,
            PortalSettingKeys.SectionPlans,
            PortalSettingKeys.SectionComparison,
            PortalSettingKeys.SectionDashboard,
            PortalSettingKeys.SectionUseCases,
            PortalSettingKeys.SectionMigration,
            PortalSettingKeys.SectionProjects,
            PortalSettingKeys.SectionTestimonials,
            PortalSettingKeys.SectionFaq,
            PortalSettingKeys.SectionCta,
            PortalSettingKeys.SectionLocations,
            PortalSettingKeys.SectionPerformance,
            PortalSettingKeys.SectionSecurity,
            PortalSettingKeys.SectionApps,
            PortalSettingKeys.SectionGuarantees,
            PortalSettingKeys.SectionSupport,
            PortalSettingKeys.SectionTrusted,
        ];

        Assert.Equal(20, sections.Length);
        Assert.All(sections, key => Assert.StartsWith("portal.sections.", key));
        foreach (var key in sections)
        {
            var seeded = PortalSettingKeys.Defaults.Single(d => d.Key == key);
            Assert.Equal(key == PortalSettingKeys.SectionProjects ? "false" : "true", seeded.Value);
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.False(PortalSettingKeys.AllowsEmpty(key), $"{key} always has a value");
            Assert.Same(PortalSettingKeys.Booleans, PortalSettingKeys.AllowedValues[key]);
        }
    }

    /// <summary>The performance, security, installer, guarantee and support-response keys seed empty and public so nothing is invented.</summary>
    [Fact]
    public void HostingFactKeys_AreSeededEmptyAndPublic()
    {
        string[] keys =
        [
            PortalSettingKeys.StatsResponseTime,
            PortalSettingKeys.StatsSitesHosted,
            PortalSettingKeys.StatsSupportResponse,
            PortalSettingKeys.FeaturesSsl,
            PortalSettingKeys.FeaturesDdos,
            PortalSettingKeys.InstallerAppCount,
            PortalSettingKeys.GuaranteeMoneyBackDays,
            PortalSettingKeys.GuaranteeNoHiddenFees,
            PortalSettingKeys.SupportResponseChat,
            PortalSettingKeys.SupportResponseTicket,
            PortalSettingKeys.SupportResponseEmail,
            PortalSettingKeys.SupportResponsePhone,
        ];

        foreach (var key in keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.True(PortalSettingKeys.AllowsEmpty(key), $"{key} must seed empty so no claim is invented");
        }

        Assert.Same(PortalSettingKeys.Booleans, PortalSettingKeys.AllowedValues[PortalSettingKeys.FeaturesSsl]);
        Assert.Same(PortalSettingKeys.Booleans, PortalSettingKeys.AllowedValues[PortalSettingKeys.FeaturesDdos]);
        Assert.Same(PortalSettingKeys.Booleans, PortalSettingKeys.AllowedValues[PortalSettingKeys.GuaranteeNoHiddenFees]);
    }

    /// <summary>The money-back days accept one to three digits and nothing else.</summary>
    /// <param name="value">The value to test.</param>
    /// <param name="valid">Whether the shape should accept it.</param>
    [Theory]
    [InlineData("30", true)]
    [InlineData("7", true)]
    [InlineData("365", true)]
    [InlineData("1000", false)]
    [InlineData("30 days", false)]
    [InlineData("-5", false)]
    public void MoneyBackDays_PatternAcceptsOnlyShortDigitStrings(string value, bool valid)
    {
        var pattern = PortalSettingKeys.Patterns[PortalSettingKeys.GuaranteeMoneyBackDays];

        Assert.Equal(valid, pattern.Regex.IsMatch(value));
    }
}
