using FluentAssertions;
using Innovayse.Hostpanel.Application.Reports.Common;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>Pins the period resolution every monthly report shares.</summary>
public sealed class ReportMonthTests
{
    /// <summary>The fixed moment the clock-defaulting tests resolve against.</summary>
    private static readonly DateTimeOffset Now = new(2026, 7, 15, 10, 0, 0, TimeSpan.Zero);

    /// <summary>An omitted year and month default to the clock's.</summary>
    [Fact]
    public void Resolve_WithNothingRequested_UsesTheCurrentMonth()
    {
        var period = ReportMonth.Resolve(year: null, month: null, Now);

        period.Should().Be(new ReportMonth(2026, 7));
    }

    /// <summary>A requested year and month win over the clock, each independently.</summary>
    [Fact]
    public void Resolve_WithAYearOnly_KeepsTheClockMonth()
    {
        var period = ReportMonth.Resolve(year: 2024, month: null, Now);

        period.Should().Be(new ReportMonth(2024, 7));
    }

    /// <summary>The window is the whole month, half-open at the end.</summary>
    [Fact]
    public void Window_SpansTheMonthHalfOpen()
    {
        var period = new ReportMonth(2026, 2);

        period.From.Should().Be(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));
        period.To.Should().Be(new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero));
        period.DaysInMonth.Should().Be(28);
    }

    /// <summary>December's window ends on January 1 of the next year.</summary>
    [Fact]
    public void Window_ForDecember_RollsIntoTheNextYear()
    {
        var period = new ReportMonth(2026, 12);

        period.To.Should().Be(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
