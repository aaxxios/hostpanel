using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Billing.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Billing;

/// <summary>
/// Payments that do not cover the whole invoice: the invoice stays open and keeps taking payments
/// until the money covers <see cref="Invoice.Total"/>, and every path that settles the rest charges
/// and records only <see cref="Invoice.AmountDue"/>.
/// </summary>
public sealed class InvoicePartialPaymentTests
{
    private static readonly DateTimeOffset Today = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Builds an open invoice for 10.00 with its creation events cleared.</summary>
    /// <returns>The invoice.</returns>
    private static Invoice OpenInvoice()
    {
        var invoice = Invoice.Create(1, Today.AddDays(7), "USD");
        invoice.AddItem("Hosting", 10m, 1);
        invoice.ClearDomainEvents();
        return invoice;
    }

    /// <summary>A payment short of the total leaves the invoice Unpaid, owing the rest, with no payment event.</summary>
    [Fact]
    public void PartialPayment_KeepsTheInvoiceOpen()
    {
        var invoice = OpenInvoice();

        invoice.AddPayment(Today, "Bank", "tx1", 4m, 0m, null);

        invoice.Status.Should().Be(InvoiceStatus.Unpaid);
        invoice.AmountDue.Should().Be(6m);
        invoice.PaidAt.Should().BeNull();
        invoice.DomainEvents.OfType<PaymentReceivedEvent>().Should().BeEmpty(
            "fulfilment, renewals and the receipt mail run once, when the invoice is paid in full");
    }

    /// <summary>A partial payment on an overdue invoice leaves it Overdue: the debt is still late.</summary>
    [Fact]
    public void PartialPayment_OnOverdueInvoice_LeavesItOverdue()
    {
        var invoice = OpenInvoice();
        invoice.MarkOverdue();

        invoice.AddPayment(Today, "Bank", "tx1", 4m, 0m, null);

        invoice.Status.Should().Be(InvoiceStatus.Overdue);
        invoice.AmountDue.Should().Be(6m);
    }

    /// <summary>The payment that covers the rest marks the invoice Paid and raises the payment event once.</summary>
    [Fact]
    public void SecondPayment_CompletesTheInvoice()
    {
        var invoice = OpenInvoice();
        invoice.AddPayment(Today, "Bank", "tx1", 4m, 0m, null);

        invoice.AddPayment(Today.AddDays(1), "Bank", "tx2", 6m, 0m, null);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.AmountDue.Should().Be(0m);
        invoice.IsPaidInFull.Should().BeTrue();
        invoice.PaidAt.Should().Be(Today.AddDays(1));
        invoice.DomainEvents.OfType<PaymentReceivedEvent>().Should().ContainSingle();
    }

    /// <summary>
    /// A payment above the amount due marks the invoice Paid; the excess stays on the recorded
    /// transaction for staff to settle, and is not turned into credit on its own.
    /// </summary>
    [Fact]
    public void Overpayment_MarksTheInvoicePaid()
    {
        var invoice = OpenInvoice();

        invoice.AddPayment(Today, "Bank", "tx1", 15m, 0m, null);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.AmountDue.Should().Be(0m);
        invoice.AmountPaid.Should().Be(15m);
    }

    /// <summary>A Paid invoice takes no further payment.</summary>
    [Fact]
    public void Payment_OnPaidInvoice_IsRefused()
    {
        var invoice = OpenInvoice();
        invoice.AddPayment(Today, "Bank", "tx1", 10m, 0m, null);

        var act = () => invoice.AddPayment(Today, "Bank", "tx2", 1m, 0m, null);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceNotPayable);
        invoice.AmountPaid.Should().Be(10m);
    }

    /// <summary>
    /// Settling the rest through the invoice's gateway session records that remainder as a payment,
    /// so the recorded payments cover the total and the renewal engine sees it paid in full.
    /// </summary>
    [Fact]
    public void GatewayCompletion_AfterPartialPayment_RecordsTheRemainder()
    {
        var invoice = OpenInvoice();
        invoice.AddPayment(Today, "Bank", "tx1", 4m, 0m, null);
        invoice.SetGatewaySession("bank-gateway", "gw-1");

        invoice.MarkPaidViaGateway("gw-tx");

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.IsPaidInFull.Should().BeTrue();
        invoice.Transactions.Where(t => t.Type == InvoiceTransactionType.Payment).Select(t => t.Amount)
            .Should().BeEquivalentTo([4m, 6m]);
    }

    /// <summary>A full-charge payment after a partial one records only the remainder, too.</summary>
    [Fact]
    public void MarkPaid_AfterPartialPayment_RecordsTheRemainder()
    {
        var invoice = OpenInvoice();
        invoice.AddPayment(Today, "Bank", "tx1", 4m, 0m, null);

        invoice.MarkPaid("card-tx", "Card");

        invoice.IsPaidInFull.Should().BeTrue();
        invoice.Transactions.Where(t => t.Type == InvoiceTransactionType.Payment).Sum(t => t.Amount).Should().Be(10m);
    }

    /// <summary>An invoice paid in one go through a full-charge path records nothing extra, as before.</summary>
    [Fact]
    public void MarkPaid_WithoutEarlierPayments_RecordsNoTransaction()
    {
        var invoice = OpenInvoice();

        invoice.MarkPaid("card-tx");

        invoice.IsPaidInFull.Should().BeTrue();
        invoice.Transactions.Should().BeEmpty();
    }

    /// <summary>
    /// Credit that covers what a partial payment left settles the invoice: nothing is owed, so it
    /// must not stay open, go overdue and suspend a client who has paid.
    /// </summary>
    [Fact]
    public void CreditCoveringTheRest_SettlesTheInvoice()
    {
        var invoice = OpenInvoice();
        invoice.AddPayment(Today, "Bank", "tx1", 4m, 0m, null);

        invoice.ApplyCredit(6m);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.AmountDue.Should().Be(0m);
        invoice.DomainEvents.OfType<PaymentReceivedEvent>().Should().ContainSingle();
    }

    /// <summary>Removing a line so that the payments cover what is left settles the invoice.</summary>
    [Fact]
    public void RemovingALine_SoPaymentsCoverTheRest_SettlesTheInvoice()
    {
        // Lines are not persisted here, so every id is 0 and RemoveItem takes the first one: the
        // removed line is added first on purpose.
        var invoice = Invoice.Create(1, Today.AddDays(7), "USD");
        invoice.AddItem("Setup", 5m, 1);
        invoice.AddItem("Hosting", 10m, 1);
        invoice.AddPayment(Today, "Bank", "tx1", 10m, 0m, null);
        invoice.Status.Should().Be(InvoiceStatus.Unpaid, "10 of 15 is paid");

        invoice.RemoveItem(invoice.Items[0].Id);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    /// <summary>
    /// Marking a paid invoice Unpaid (the money bounced) reverses what was paid, so the whole total
    /// is due again and the invoice can be paid once more.
    /// </summary>
    [Fact]
    public void MarkUnpaid_ReversesThePayments_SoTheTotalIsDueAgain()
    {
        var invoice = OpenInvoice();
        invoice.AddPayment(Today, "Bank", "tx1", 10m, 0m, null);

        invoice.MarkUnpaid();

        invoice.Status.Should().Be(InvoiceStatus.Unpaid);
        invoice.AmountPaid.Should().Be(0m);
        invoice.AmountDue.Should().Be(10m);

        invoice.AddPayment(Today.AddDays(2), "Bank", "tx2", 10m, 0m, null);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    /// <summary>A closed invoice owes nothing, whatever its transactions add up to.</summary>
    [Fact]
    public void AmountDue_OnACancelledInvoice_IsZero()
    {
        var invoice = OpenInvoice();
        invoice.AddPayment(Today, "Bank", "tx1", 4m, 0m, null);

        invoice.Cancel();

        invoice.AmountDue.Should().Be(0m);
    }
}
