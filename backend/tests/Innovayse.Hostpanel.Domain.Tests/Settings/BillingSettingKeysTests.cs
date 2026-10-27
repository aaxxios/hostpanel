using FluentAssertions;
using Innovayse.Hostpanel.Domain.Settings;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Settings;

/// <summary>The billing setting vocabulary.</summary>
public sealed class BillingSettingKeysTests
{
    /// <summary>Every default passes its own pattern.</summary>
    [Fact]
    public void Defaults_MatchTheirPatterns()
    {
        foreach (var (key, value, _) in BillingSettingKeys.Defaults)
        {
            BillingSettingKeys.Patterns[key].Regex.IsMatch(value).Should().BeTrue(key);
        }
    }

    /// <summary>The day lists accept what an operator types and refuse the rest.</summary>
    [Theory]
    [InlineData("7,3,1", true)]
    [InlineData("14", true)]
    [InlineData("7, 3", true)]
    [InlineData("0", false)]
    [InlineData("91", false)]
    [InlineData("a,b", false)]
    public void ReminderDays_Pattern(string value, bool ok)
    {
        BillingSettingKeys.Patterns[BillingSettingKeys.ReminderDays].Regex.IsMatch(value).Should().Be(ok);
    }

    /// <summary><see cref="BillingSettingKeys.MaxDays"/> is exactly the largest day count the patterns accept.</summary>
    [Fact]
    public void MaxDays_IsThePatternsUpperBound()
    {
        var max = BillingSettingKeys.MaxDays.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var over = (BillingSettingKeys.MaxDays + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        foreach (var key in new[] { BillingSettingKeys.InvoiceDaysBefore, BillingSettingKeys.ReminderDays })
        {
            BillingSettingKeys.Patterns[key].Regex.IsMatch(max).Should().BeTrue(key);
            BillingSettingKeys.Patterns[key].Regex.IsMatch(over).Should().BeFalse(key);
        }
    }

    /// <summary>Reminders can be switched off by clearing the list; the other keys cannot be emptied.</summary>
    [Fact]
    public void OnlyReminderDays_AllowsEmpty()
    {
        BillingSettingKeys.AllowsEmpty(BillingSettingKeys.ReminderDays).Should().BeTrue();
        BillingSettingKeys.AllowsEmpty(BillingSettingKeys.InvoiceDaysBefore).Should().BeFalse();
    }

    /// <summary>No billing key leaks into the anonymous storefront settings.</summary>
    [Fact]
    public void NoBillingKeyIsPublic()
    {
        BillingSettingKeys.Defaults.Select(d => d.Key).Should().NotIntersectWith(PortalSettingKeys.Public);
    }
}
