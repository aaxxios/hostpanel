using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Billing.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Xunit;
namespace Innovayse.Hostpanel.Domain.Tests.Billing;
/// <summary>Unit tests for the <see cref="Invoice"/> aggregate.</summary>
public sealed class InvoiceTests
{
    /// <summary>Create sets all properties and raises InvoiceCreatedEvent.</summary>
    [Fact]
    public void Create_SetsPropertiesAndRaisesEvent()
    {
        var due = DateTimeOffset.UtcNow.AddDays(14);

        var invoice = Invoice.Create(clientId: 7, dueDate: due, currency: "USD");

        invoice.ClientId.Should().Be(7);
        invoice.Status.Should().Be(InvoiceStatus.Unpaid);
        invoice.DueDate.Should().Be(due);
        invoice.Total.Should().Be(0m);
        invoice.PaidAt.Should().BeNull();
        invoice.GatewayTransactionId.Should().BeNull();
        invoice.Items.Should().BeEmpty();
        invoice.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InvoiceCreatedEvent>()
            .Which.ClientId.Should().Be(7);
    }

    /// <summary>AddItem increases Total and appends item to collection.</summary>
    [Fact]
    public void AddItem_IncreasesTotalAndAddsItem()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");

        invoice.AddItem("Hosting Plan", unitPrice: 10m, quantity: 2);

        invoice.Total.Should().Be(20m);
        invoice.Items.Should().HaveCount(1);
        invoice.Items[0].Description.Should().Be("Hosting Plan");
        invoice.Items[0].UnitPrice.Should().Be(10m);
        invoice.Items[0].Quantity.Should().Be(2);
        invoice.Items[0].Amount.Should().Be(20m);
    }

    /// <summary>AddItem twice accumulates Total correctly.</summary>
    [Fact]
    public void AddItem_Twice_AccumulatesTotal()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");

        invoice.AddItem("Domain", 12m, 1);
        invoice.AddItem("SSL", 5m, 3);

        invoice.Total.Should().Be(27m);
        invoice.Items.Should().HaveCount(2);
    }

    /// <summary>MarkPaid sets status, PaidAt, transactionId and raises PaymentReceivedEvent.</summary>
    [Fact]
    public void MarkPaid_SetsStatusAndRaisesEvent()
    {
        var invoice = Invoice.Create(clientId: 3, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");
        invoice.AddItem("VPS", 20m, 1);
        invoice.ClearDomainEvents();

        invoice.MarkPaid("txn_abc123");

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAt.Should().NotBeNull();
        invoice.GatewayTransactionId.Should().Be("txn_abc123");
        invoice.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentReceivedEvent>()
            .Which.TransactionId.Should().Be("txn_abc123");
    }

    /// <summary>MarkPaid on a non-payable invoice throws <see cref="DomainRuleException"/> with <c>INVOICE_NOT_PAYABLE</c>.</summary>
    [Fact]
    public void MarkPaid_WhenAlreadyPaid_ThrowsWithInvoiceNotPayable()
    {
        var invoice = Invoice.Create(clientId: 3, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");
        invoice.MarkPaid("txn_1");

        var act = () => invoice.MarkPaid("txn_2");

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceNotPayable);
    }

    /// <summary>MarkOverdue transitions from Unpaid to Overdue and raises InvoiceOverdueEvent.</summary>
    [Fact]
    public void MarkOverdue_SetsOverdueAndRaisesEvent()
    {
        var invoice = Invoice.Create(clientId: 5, dueDate: DateTimeOffset.UtcNow.AddDays(-1), currency: "USD");
        invoice.ClearDomainEvents();

        invoice.MarkOverdue();

        invoice.Status.Should().Be(InvoiceStatus.Overdue);
        invoice.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InvoiceOverdueEvent>();
    }

    /// <summary>Cancel changes status to Cancelled for an Unpaid invoice.</summary>
    [Fact]
    public void Cancel_WhenUnpaid_SetsCancelled()
    {
        var invoice = Invoice.Create(clientId: 2, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");

        invoice.Cancel();

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    /// <summary>Cancel on a Paid invoice throws <see cref="DomainRuleException"/> with <c>INVOICE_CANCEL_NOT_ALLOWED</c>.</summary>
    [Fact]
    public void Cancel_WhenPaid_ThrowsWithInvoiceCancelNotAllowed()
    {
        var invoice = Invoice.Create(clientId: 2, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");
        invoice.MarkPaid("txn_99");

        var act = () => invoice.Cancel();

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceCancelNotAllowed);
    }

    /// <summary>MarkOverdue called twice is idempotent — no duplicate event raised.</summary>
    [Fact]
    public void MarkOverdue_WhenAlreadyOverdue_IsIdempotent()
    {
        var invoice = Invoice.Create(clientId: 5, dueDate: DateTimeOffset.UtcNow.AddDays(-1), currency: "USD");
        invoice.MarkOverdue();
        invoice.ClearDomainEvents();

        var act = () => invoice.MarkOverdue();

        act.Should().NotThrow();
        invoice.DomainEvents.Should().BeEmpty();
    }

    /// <summary>AddItem on an Overdue invoice throws <see cref="DomainRuleException"/> with <c>INVOICE_NOT_EDITABLE</c>.</summary>
    [Fact]
    public void AddItem_WhenOverdue_ThrowsWithInvoiceNotEditable()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(-1), currency: "USD");
        invoice.MarkOverdue();

        var act = () => invoice.AddItem("SSL", 5m, 1);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceNotEditable);
    }

    /// <summary>Publish on a non-Draft invoice throws <see cref="DomainRuleException"/> with <c>INVOICE_NOT_DRAFT</c>.</summary>
    [Fact]
    public void Publish_WhenNotDraft_ThrowsWithInvoiceNotDraft()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");

        var act = invoice.Publish;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceNotDraft);
    }

    /// <summary>
    /// RemoveItem with an id that names no line on the invoice throws <see cref="DomainRuleException"/>
    /// with <c>INVOICE_ITEM_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public void RemoveItem_WhenIdUnknown_ThrowsWithInvoiceItemNotFound()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");

        var act = () => invoice.RemoveItem(999);

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceItemNotFound);
    }

    /// <summary>
    /// MarkOverdue on a Draft invoice throws <see cref="DomainRuleException"/> with <c>INVOICE_NOT_UNPAID</c>.
    /// </summary>
    [Fact]
    public void MarkOverdue_WhenDraft_ThrowsWithInvoiceNotUnpaid()
    {
        var invoice = Invoice.CreateDraft(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(-1), currency: "USD");

        var act = invoice.MarkOverdue;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceNotUnpaid);
    }

    /// <summary>MarkUnpaid on an Unpaid invoice throws <see cref="DomainRuleException"/> with <c>INVOICE_NOT_PAID</c>.</summary>
    [Fact]
    public void MarkUnpaid_WhenNotPaid_ThrowsWithInvoiceNotPaid()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");

        var act = invoice.MarkUnpaid;

        act.Should().Throw<DomainRuleException>().Which.Code.Should().Be(BillingErrors.InvoiceNotPaid);
    }

    /// <summary>CreateForBillableItems builds an unpaid invoice with one single-quantity line per item.</summary>
    [Fact]
    public void CreateForBillableItems_AddsOneLinePerItemAtItsAmount()
    {
        var due = DateTimeOffset.UtcNow.AddDays(14);
        var items = new[]
        {
            BillableItem.Create(7, "Setup fee", 25m, "USD", BillableItemType.OneTime),
            BillableItem.Create(7, "Extra IP", 4.5m, "USD", BillableItemType.OneTime),
        };

        var invoice = Invoice.CreateForBillableItems(clientId: 7, dueDate: due, currency: "USD", items);

        invoice.ClientId.Should().Be(7);
        invoice.Status.Should().Be(InvoiceStatus.Unpaid);
        invoice.DueDate.Should().Be(due);
        invoice.Items.Select(i => (i.Description, i.Quantity, i.Amount))
            .Should().Equal(("Setup fee", 1, 25m), ("Extra IP", 1, 4.5m));
        invoice.Total.Should().Be(29.5m);
        invoice.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<InvoiceCreatedEvent>();
    }

    /// <summary>CreateForBillableItems with no items yields an empty unpaid invoice.</summary>
    [Fact]
    public void CreateForBillableItems_WithNoItems_YieldsEmptyInvoice()
    {
        var invoice = Invoice.CreateForBillableItems(
            clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD", []);

        invoice.Items.Should().BeEmpty();
        invoice.Total.Should().Be(0m);
    }

    /// <summary>The currency given at creation is what the invoice reports, upper-cased.</summary>
    [Fact]
    public void Create_StampsTheCurrency()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "usd");
        Assert.Equal("USD", invoice.Currency);
    }

    /// <summary>A draft carries its currency too — it is not decided later at "send".</summary>
    [Fact]
    public void CreateDraft_StampsTheCurrency()
    {
        var invoice = Invoice.CreateDraft(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "AMD");
        Assert.Equal("AMD", invoice.Currency);
    }

    /// <summary>The four-argument overload stamps the currency on both a draft and an unpaid invoice.</summary>
    /// <param name="isDraft">Whether the invoice is created as a draft.</param>
    /// <param name="expected">The status the invoice should start in.</param>
    [Theory]
    [InlineData(true, InvoiceStatus.Draft)]
    [InlineData(false, InvoiceStatus.Unpaid)]
    public void Create_WithDraftFlag_StampsTheCurrencyAndStatus(bool isDraft, InvoiceStatus expected)
    {
        var invoice = Invoice.Create(1, DateTimeOffset.UtcNow.AddDays(7), "eur", isDraft);
        Assert.Equal("EUR", invoice.Currency);
        Assert.Equal(expected, invoice.Status);
    }

    /// <summary>A duplicate bills the same lines in the same currency as the original.</summary>
    [Fact]
    public void Duplicate_KeepsTheCurrency()
    {
        var original = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "AMD");
        original.AddItem("Hosting", 1000m, 1);

        var copy = original.Duplicate();

        Assert.Equal("AMD", copy.Currency);
        Assert.Equal(InvoiceStatus.Draft, copy.Status);
    }

    /// <summary>An invoice with no currency is the bug this field exists to end.</summary>
    /// <param name="currency">A missing or malformed currency code.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Create_RejectsAMissingOrMalformedCurrency(string currency)
        => Assert.Throws<ArgumentException>(() => Invoice.Create(1, DateTimeOffset.UtcNow.AddDays(7), currency));

    /// <summary>Set gateway session, on unpaid invoice, stores module order id and timestamp.</summary>
    [Fact]
    public void SetGatewaySession_OnUnpaidInvoice_StoresModuleOrderIdAndTimestamp()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(14), currency: "USD");
        var before = DateTimeOffset.UtcNow;

        invoice.SetGatewaySession("bank-gateway", "32faa424-858a-4f22");

        Assert.Equal("bank-gateway", invoice.GatewayModule);
        Assert.Equal("32faa424-858a-4f22", invoice.GatewayOrderId);
        Assert.NotNull(invoice.GatewayStartedAt);
        Assert.True(invoice.GatewayStartedAt >= before);
    }

    /// <summary>Set gateway session, second attempt, overwrites previous session.</summary>
    [Fact]
    public void SetGatewaySession_SecondAttempt_OverwritesPreviousSession()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(14), currency: "USD");
        invoice.SetGatewaySession("bank-gateway", "first-attempt");

        invoice.SetGatewaySession("bank-gateway", "second-attempt");

        Assert.Equal("second-attempt", invoice.GatewayOrderId);
    }

    /// <summary>Set gateway session, on paid invoice, throws.</summary>
    [Fact]
    public void SetGatewaySession_OnPaidInvoice_Throws()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(14), currency: "USD");
        invoice.SetGatewaySession("bank-gateway", "attempt-1");
        invoice.MarkPaid("txn-1");

        Assert.Throws<InvalidOperationException>(
            () => invoice.SetGatewaySession("bank-gateway", "attempt-2"));
    }

    /// <summary>Mark paid, with stale gateway session, clears it.</summary>
    [Fact]
    public void MarkPaid_WithStaleGatewaySession_ClearsIt()
    {
        // Payer started an hosted gateway session, abandoned it, then paid a different way
        // (e.g. card payment gateway, or an admin recorded a manual payment). The stale session must not
        // survive — otherwise refund/reconciliation code would later mistake it for the
        // gateway that actually took the money.
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(14), currency: "USD");
        invoice.SetGatewaySession("bank-gateway", "abandoned-session");

        invoice.MarkPaid("card-txn-1");

        Assert.Null(invoice.GatewayModule);
        Assert.Null(invoice.GatewayOrderId);
        Assert.Null(invoice.GatewayStartedAt);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    /// <summary>Mark paid via gateway, retains the active session.</summary>
    [Fact]
    public void MarkPaidViaGateway_RetainsTheActiveSession()
    {
        // The completion path for the invoice's own live gateway session — the session fields
        // must be retained so refund/reconciliation code can identify which gateway paid.
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(14), currency: "USD");
        invoice.SetGatewaySession("bank-gateway", "gw-order-1");

        invoice.MarkPaidViaGateway("gw-order-1");

        Assert.Equal("bank-gateway", invoice.GatewayModule);
        Assert.Equal("gw-order-1", invoice.GatewayOrderId);
        Assert.NotNull(invoice.GatewayStartedAt);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    /// <summary>MarkPaidViaGateway on an already-paid invoice refuses with <c>INVOICE_NOT_PAYABLE</c>.</summary>
    [Fact]
    public void MarkPaidViaGateway_OnPaidInvoice_ThrowsWithInvoiceNotPayable()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(14), currency: "USD");
        invoice.SetGatewaySession("bank-gateway", "gw-order-1");
        invoice.MarkPaidViaGateway("gw-order-1");

        var ex = Assert.Throws<DomainRuleException>(() => invoice.MarkPaidViaGateway("gw-order-2"));
        Assert.Equal(BillingErrors.InvoiceNotPayable, ex.Code);
    }

    /// <summary>Mark unpaid, on gateway paid invoice, clears session so reconciler cannot find it.</summary>
    [Fact]
    public void MarkUnpaid_OnGatewayPaidInvoice_ClearsSessionSoReconcilerCannotFindIt()
    {
        // An admin reversing a gateway-completed payment must leave no session behind: the
        // reconciler looks back 24 hours for pending gateway sessions, and a leftover session
        // here would let it re-query the gateway and silently re-mark the invoice Paid again,
        // undoing the admin's reversal.
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(14), currency: "USD");
        invoice.SetGatewaySession("bank-gateway", "gw-order-1");
        invoice.MarkPaidViaGateway("gw-order-1");

        invoice.MarkUnpaid();

        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        Assert.Null(invoice.GatewayModule);
        Assert.Null(invoice.GatewayOrderId);
        Assert.Null(invoice.GatewayStartedAt);
        Assert.Null(invoice.GatewayTransactionId);
    }

    /// <summary>A payment recorded with a past date stamps the invoice as paid on that date.</summary>
    [Fact]
    public void AddPayment_WithPastDate_StampsPaidAtWithThatDate()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");
        invoice.AddItem("Hosting Plan", unitPrice: 10m, quantity: 1);
        var paidOn = DateTimeOffset.UtcNow.AddMonths(-3);

        invoice.AddPayment(paidOn, "card", "txn-1", 10m, 0m, null);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAt.Should().Be(paidOn);
    }

    /// <summary>Backdating moves both the creation and the issue date.</summary>
    [Fact]
    public void Backdate_MovesCreatedAtAndInvoiceDate()
    {
        var invoice = Invoice.Create(clientId: 1, dueDate: DateTimeOffset.UtcNow.AddDays(7), currency: "USD");
        var earlier = DateTimeOffset.UtcNow.AddMonths(-5);

        invoice.Backdate(earlier);

        invoice.CreatedAt.Should().Be(earlier);
        invoice.InvoiceDate.Should().Be(earlier);
    }
}
