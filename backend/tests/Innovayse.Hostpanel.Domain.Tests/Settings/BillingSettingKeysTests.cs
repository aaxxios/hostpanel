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

    /// <summary>The lead time starts at one day; the overdue grace may be zero. Both stop at 90.</summary>
    /// <param name="value">The candidate value.</param>
    /// <param name="leadOk">Whether invoice_days_before accepts it.</param>
    /// <param name="graceOk">Whether suspend_after_overdue_days accepts it.</param>
    [Theory]
    [InlineData("0", false, true)]
    [InlineData("1", true, true)]
    [InlineData("14", true, true)]
    [InlineData("90", true, true)]
    [InlineData("91", false, false)]
    [InlineData("01", false, false)]
    public void DayCount_Patterns(string value, bool leadOk, bool graceOk)
    {
        BillingSettingKeys.Patterns[BillingSettingKeys.InvoiceDaysBefore].Regex.IsMatch(value).Should().Be(leadOk);
        BillingSettingKeys.Patterns[BillingSettingKeys.SuspendAfterOverdueDays].Regex.IsMatch(value).Should().Be(graceOk);
    }

    /// <summary>Each day-count key says its own range in the refusal text.</summary>
    [Fact]
    public void DayCount_ExpectedText()
    {
        BillingSettingKeys.Patterns[BillingSettingKeys.InvoiceDaysBefore].Expected.Should().Be("a whole number of days from 1 to 90");
        BillingSettingKeys.Patterns[BillingSettingKeys.SuspendAfterOverdueDays].Expected.Should().Be("a whole number of days from 0 to 90");
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
