using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>Cycle arithmetic for renewal periods: the length of each cycle and month-end anchoring.</summary>
public sealed class RenewalPeriodTests
{
    /// <summary>Each cycle adds its own number of months.</summary>
    /// <param name="cycle">The billing cycle.</param>
    /// <param name="months">The expected length in months.</param>
    [Theory]
    [InlineData(BillingCycle.Monthly, 1)]
    [InlineData(BillingCycle.Annual, 12)]
    public void EndOf_EachCycle_AddsItsMonths(BillingCycle cycle, int months)
    {
        var start = new DateTimeOffset(2026, 3, 10, 0, 0, 0, TimeSpan.Zero);

        RenewalPeriod.EndOf(start, cycle, anchorDay: 10).Should().Be(start.AddMonths(months));
    }

    /// <summary>A month-end anchor survives February: Jan 31 to Feb 28 to Mar 31, not Mar 28.</summary>
    [Fact]
    public void EndOf_MonthEndAnchor_DoesNotDrift()
    {
        var feb = RenewalPeriod.EndOf(new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero), BillingCycle.Monthly, 31);
        var mar = RenewalPeriod.EndOf(feb, BillingCycle.Monthly, 31);

        feb.Should().Be(new DateTimeOffset(2026, 2, 28, 0, 0, 0, TimeSpan.Zero));
        mar.Should().Be(new DateTimeOffset(2026, 3, 31, 0, 0, 0, TimeSpan.Zero));
    }

    /// <summary>A date an admin moved off the anchor keeps its own day rather than snapping back.</summary>
    [Fact]
    public void EndOf_DateEditedOffAnchor_KeepsItsDay()
    {
        var start = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);

        RenewalPeriod.EndOf(start, BillingCycle.Monthly, anchorDay: 1).Should().Be(new DateTimeOffset(2026, 2, 20, 0, 0, 0, TimeSpan.Zero));
    }

    /// <summary>
    /// A start that is not a clamped month end gets exactly one cycle, even when it lies before the
    /// anchor day: Nov 5 with anchor 26 ends Dec 5, not Dec 26.
    /// </summary>
    [Fact]
    public void EndOf_StartBeforeAnchor_NotMonthEnd_IsExactlyOneCycle()
    {
        var start = new DateTimeOffset(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);

        RenewalPeriod.EndOf(start, BillingCycle.Monthly, anchorDay: 26).Should().Be(new DateTimeOffset(2026, 12, 5, 0, 0, 0, TimeSpan.Zero));
    }

    /// <summary>An annual service bought on Feb 29 that renewed on Feb 28 of a common year returns to Feb 29 in the leap year.</summary>
    [Fact]
    public void EndOf_AnnualLeapDayAnchor_ReturnsToFeb29()
    {
        var start = new DateTimeOffset(2027, 2, 28, 0, 0, 0, TimeSpan.Zero);

        RenewalPeriod.EndOf(start, BillingCycle.Annual, anchorDay: 29).Should().Be(new DateTimeOffset(2028, 2, 29, 0, 0, 0, TimeSpan.Zero));
    }

    /// <summary>Domain periods are whole years, and a non-positive period counts as one year.</summary>
    /// <param name="years">The requested period.</param>
    /// <param name="expected">The years actually added.</param>
    [Theory]
    [InlineData(2, 2)]
    [InlineData(0, 1)]
    public void EndOfYears_AddsYears(int years, int expected)
    {
        var start = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);

        RenewalPeriod.EndOfYears(start, years).Should().Be(start.AddYears(expected));
    }
}
