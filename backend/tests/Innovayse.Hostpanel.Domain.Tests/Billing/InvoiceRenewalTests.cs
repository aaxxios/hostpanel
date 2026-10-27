using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>Renewal lines on <see cref="Invoice"/>: construction, the applied marker, copies, full payment.</summary>
public sealed class InvoiceRenewalTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>A service renewal line carries its period and service, and is a renewal.</summary>
    [Fact]
    public void AddServiceRenewal_RecordsPeriodAndService()
    {
        var invoice = Invoice.Create(1, Start, "USD");

        invoice.AddServiceRenewal("Renewal: Starter", 5m, 42, Start, Start.AddMonths(1));

        var line = invoice.Items.Single();
        line.IsRenewal.Should().BeTrue();
        line.ClientServiceId.Should().Be(42);
        line.RegisteredDomainId.Should().BeNull();
        line.RenewalPeriodStart.Should().Be(Start);
        line.RenewalPeriodEnd.Should().Be(Start.AddMonths(1));
        invoice.Total.Should().Be(5m);
    }

    /// <summary>A period that ends before it starts is a programming error.</summary>
    [Fact]
    public void AddDomainRenewal_EmptyPeriod_Throws()
    {
        var invoice = Invoice.Create(1, Start, "USD");

        var act = () => invoice.AddDomainRenewal("Renewal: a.com", 10m, 7, Start, Start);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>The applied marker can be set once, and only on a renewal line.</summary>
    [Fact]
    public void MarkRenewalApplied_Once()
    {
        var invoice = Invoice.Create(1, Start, "USD");
        invoice.AddServiceRenewal("r", 5m, 42, Start, Start.AddMonths(1));
        invoice.AddItem("plain", 1m, 1);
        var renewal = invoice.Items[0];

        renewal.MarkRenewalApplied(Start);

        renewal.RenewalAppliedAt.Should().Be(Start);
        ((Action)(() => renewal.MarkRenewalApplied(Start))).Should().Throw<InvalidOperationException>();
        ((Action)(() => invoice.Items[1].MarkRenewalApplied(Start))).Should().Throw<InvalidOperationException>();
    }

    /// <summary>A duplicated invoice keeps the service link but is not a renewal -- paying it extends nothing.</summary>
    [Fact]
    public void Duplicate_DropsThePeriod()
    {
        var invoice = Invoice.Create(1, Start, "USD");
        invoice.AddServiceRenewal("r", 5m, 42, Start, Start.AddMonths(1));

        var copy = invoice.Duplicate();

        copy.Items.Single().ClientServiceId.Should().Be(42);
        copy.Items.Single().IsRenewal.Should().BeFalse();
    }

    /// <summary>A partial manual payment leaves the invoice open, not paid in full; a full one is.</summary>
    [Fact]
    public void IsPaidInFull_CountsRecordedPayments()
    {
        var partial = Invoice.Create(1, Start, "USD");
        partial.AddItem("x", 10m, 1);
        partial.AddPayment(Start, "Bank", "tx1", 4m, 0m, null);

        var full = Invoice.Create(1, Start, "USD");
        full.AddItem("x", 10m, 1);
        full.AddPayment(Start, "Bank", "tx2", 10m, 0m, null);

        var gateway = Invoice.Create(1, Start, "USD");
        gateway.AddItem("x", 10m, 1);
        gateway.MarkPaid("pi_1");

        partial.IsPaidInFull.Should().BeFalse();
        full.IsPaidInFull.Should().BeTrue();
        gateway.IsPaidInFull.Should().BeTrue();
    }
}
