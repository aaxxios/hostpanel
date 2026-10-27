using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Billing.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;
namespace Innovayse.Hostpanel.Domain.Billing;
/// <summary>
/// An invoice issued to a client for services or products.
/// Owns a collection of <see cref="InvoiceItem"/> line items and <see cref="InvoiceTransaction"/> records.
/// Stored in the <c>Invoices</c> table.
/// </summary>
public sealed class Invoice : BaseEntity, IAggregateRoot
{
    /// <summary>Internal mutable list of line items.</summary>
    private readonly List<InvoiceItem> _items = [];

    /// <summary>Internal mutable list of transactions.</summary>
    private readonly List<InvoiceTransaction> _transactions = [];

    /// <summary>Gets the FK to the owning client.</summary>
    public int ClientId { get; private set; }

    /// <summary>Gets the current lifecycle status.</summary>
    public InvoiceStatus Status { get; private set; }

    /// <summary>Gets the invoice issue date (UTC).</summary>
    public DateTimeOffset InvoiceDate { get; private set; }

    /// <summary>Gets the payment due date (UTC).</summary>
    public DateTimeOffset DueDate { get; private set; }

    /// <summary>Gets the UTC timestamp when the invoice was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the UTC timestamp when payment was received; null if unpaid.</summary>
    public DateTimeOffset? PaidAt { get; private set; }

    /// <summary>Gets the sum of all line item amounts before tax and credit.</summary>
    public decimal SubTotal { get; private set; }

    /// <summary>Gets the tax rate percentage applied to the invoice.</summary>
    public decimal TaxRate { get; private set; }

    /// <summary>Gets the computed tax amount (SubTotal × TaxRate / 100).</summary>
    public decimal Tax { get; private set; }

    /// <summary>Gets the total credit applied to the invoice.</summary>
    public decimal Credit { get; private set; }

    /// <summary>Gets the final total (SubTotal + Tax − Credit).</summary>
    public decimal Total { get; private set; }

    /// <summary>
    /// ISO 4217 alpha code every amount on this invoice is in. Set at creation, never changed.
    /// </summary>
    /// <remarks>
    /// Before this existed an invoice's currency was inferred from the client at payment time,
    /// so changing a client's currency silently re-labelled every past invoice — $100 became
    /// ֏100. Billing systems document exactly that failure. The number and its currency travel together.
    /// </remarks>
    public string Currency { get; private set; } = string.Empty;

    /// <summary>Gets the payment gateway transaction reference; null until paid.</summary>
    public string? GatewayTransactionId { get; private set; }

    /// <summary>Gets the external system ID (e.g. source system invoice ID) used for migration deduplication.</summary>
    public string? ExternalId { get; private set; }

    /// <summary>Gets the payment plugin id that initiated the latest gateway payment session; null when none.</summary>
    public string? GatewayModule { get; private set; }

    /// <summary>Gets the gateway-side order id of the latest payment attempt; null when none.</summary>
    public string? GatewayOrderId { get; private set; }

    /// <summary>Gets the UTC timestamp when the latest gateway payment attempt started; null when none.</summary>
    public DateTimeOffset? GatewayStartedAt { get; private set; }

    /// <summary>Gets optional notes for this invoice.</summary>
    public string? Notes { get; private set; }

    /// <summary>Gets the preferred payment method; null when not specified.</summary>
    public string? PaymentMethod { get; private set; }

    /// <summary>Gets the read-only view of invoice line items.</summary>
    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();

    /// <summary>Gets the read-only view of invoice transactions.</summary>
    public IReadOnlyList<InvoiceTransaction> Transactions => _transactions.AsReadOnly();

    /// <summary>EF Core parameterless constructor — do not call directly.</summary>
    private Invoice() { }

    /// <summary>Creates a draft invoice (not yet sent to the client).</summary>
    /// <param name="clientId">FK to the client being invoiced.</param>
    /// <param name="dueDate">Payment due date (UTC).</param>
    /// <param name="currency">ISO 4217 alpha code the invoice bills in.</param>
    /// <returns>A new <see cref="Invoice"/> with <see cref="InvoiceStatus.Draft"/> status.</returns>
    /// <exception cref="ArgumentException">The currency is not a three-letter code.</exception>
    public static Invoice CreateDraft(int clientId, DateTimeOffset dueDate, string currency)
    {
        var now = DateTimeOffset.UtcNow;
        var invoice = new Invoice
        {
            ClientId = clientId,
            Currency = CurrencyCode.Normalise(currency),
            Status = InvoiceStatus.Draft,
            InvoiceDate = now,
            DueDate = dueDate,
            CreatedAt = now,
        };
        invoice.AddDomainEvent(() => new InvoiceCreatedEvent(invoice.Id, clientId));
        return invoice;
    }

    /// <summary>Creates an unpaid invoice.</summary>
    /// <param name="clientId">FK to the client being invoiced.</param>
    /// <param name="dueDate">Payment due date (UTC).</param>
    /// <param name="currency">ISO 4217 alpha code the invoice bills in.</param>
    /// <returns>A new <see cref="Invoice"/> with <see cref="InvoiceStatus.Unpaid"/> status.</returns>
    /// <exception cref="ArgumentException">The currency is not a three-letter code.</exception>
    public static Invoice Create(int clientId, DateTimeOffset dueDate, string currency)
    {
        // Same construction and the same InvoiceCreatedEvent as a draft; only the status differs.
        var invoice = CreateDraft(clientId, dueDate, currency);
        invoice.Status = InvoiceStatus.Unpaid;
        return invoice;
    }

    /// <summary>Creates a draft or unpaid invoice.</summary>
    /// <param name="clientId">FK to the client being invoiced.</param>
    /// <param name="dueDate">Payment due date (UTC).</param>
    /// <param name="currency">ISO 4217 alpha code the invoice bills in.</param>
    /// <param name="isDraft">When true, creates a Draft; otherwise creates an Unpaid invoice.</param>
    /// <returns>A new <see cref="Invoice"/>.</returns>
    /// <exception cref="ArgumentException">The currency is not a three-letter code.</exception>
    public static Invoice Create(int clientId, DateTimeOffset dueDate, string currency, bool isDraft) =>
        isDraft ? CreateDraft(clientId, dueDate, currency) : Create(clientId, dueDate, currency);

    /// <summary>Creates an unpaid invoice with one single-quantity line per billable item.</summary>
    /// <param name="clientId">FK to the client being invoiced.</param>
    /// <param name="dueDate">Payment due date (UTC).</param>
    /// <param name="currency">ISO 4217 alpha code the invoice bills in.</param>
    /// <param name="items">The billable items to charge, one line each at the item's amount.</param>
    /// <returns>A new <see cref="Invoice"/> with <see cref="InvoiceStatus.Unpaid"/> status and the lines added.</returns>
    /// <exception cref="ArgumentException">The currency is not a three-letter code.</exception>
    public static Invoice CreateForBillableItems(
        int clientId, DateTimeOffset dueDate, string currency, IEnumerable<BillableItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var invoice = Create(clientId, dueDate, currency);
        foreach (var item in items)
        {
            invoice.AddItem(item.Description, item.Amount, 1);
        }

        return invoice;
    }

    /// <summary>Sets the external system ID for migration deduplication.</summary>
    /// <param name="externalId">Identifier of this invoice in the system it was imported from.</param>
    public void SetExternalId(string externalId) => ExternalId = externalId;

    /// <summary>
    /// Publishes a draft invoice, transitioning it to Unpaid status.
    /// </summary>
    /// <exception cref="DomainRuleException">Thrown when the invoice is not in Draft status.</exception>
    public void Publish()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new DomainRuleException(
                BillingErrors.InvoiceNotDraft,
                $"Only Draft invoices can be published; current status is {Status}.");
        }

        Status = InvoiceStatus.Unpaid;
    }

    /// <summary>
    /// Adds a line item and recalculates totals.
    /// </summary>
    /// <param name="description">Human-readable charge description.</param>
    /// <param name="unitPrice">Price per unit.</param>
    /// <param name="quantity">Number of units.</param>
    /// <param name="clientServiceId">
    /// FK to the <c>ClientService</c> this line is charged for, or <see langword="null"/> when
    /// it is not one service's charge or the caller does not know which. Only a caller holding
    /// the service itself may pass it — see <see cref="InvoiceItem.ClientServiceId"/> for why
    /// inferring it from the description is not allowed.
    /// </param>
    /// <exception cref="DomainRuleException">Thrown when the invoice is no longer editable.</exception>
    public void AddItem(string description, decimal unitPrice, int quantity, int? clientServiceId = null)
    {
        if (Status is not (InvoiceStatus.Draft or InvoiceStatus.Unpaid))
        {
            throw new DomainRuleException(
                BillingErrors.InvoiceNotEditable, $"Cannot add items to an invoice with status {Status}.");
        }

        var item = InvoiceItem.Create(description, unitPrice, quantity, clientServiceId);
        _items.Add(item);
        RecalculateTotals();
    }

    /// <summary>Adds a renewal line for one service and one period, and recalculates totals.</summary>
    /// <param name="description">Line description.</param>
    /// <param name="amount">Price of the period.</param>
    /// <param name="clientServiceId">The service renewed.</param>
    /// <param name="start">Period start.</param>
    /// <param name="end">Period end.</param>
    /// <exception cref="DomainRuleException">The invoice is no longer editable.</exception>
    public void AddServiceRenewal(string description, decimal amount, int clientServiceId, DateTimeOffset start, DateTimeOffset end) =>
        AddRenewal(InvoiceItem.CreateRenewal(description, amount, clientServiceId, null, start, end));

    /// <summary>Adds a renewal line for one domain and one period, and recalculates totals.</summary>
    /// <param name="description">Line description.</param>
    /// <param name="amount">Price of the period.</param>
    /// <param name="registeredDomainId">The domain renewed.</param>
    /// <param name="start">Period start (the current expiry).</param>
    /// <param name="end">Period end.</param>
    /// <exception cref="DomainRuleException">The invoice is no longer editable.</exception>
    public void AddDomainRenewal(string description, decimal amount, int registeredDomainId, DateTimeOffset start, DateTimeOffset end) =>
        AddRenewal(InvoiceItem.CreateRenewal(description, amount, null, registeredDomainId, start, end));

    /// <summary>The gateway recorded on a remainder payment when the paying path names none.</summary>
    private const string UnknownGateway = "Unknown";

    /// <summary>The gateway recorded on the transaction that reverses payments when an invoice is marked unpaid.</summary>
    private const string ReversalGateway = "Reversal";

    /// <summary>
    /// Gets the money the invoice holds: the payments recorded against it less the refunds and
    /// reversals (<see cref="MarkUnpaid"/>) taken back out.
    /// </summary>
    public decimal AmountPaid =>
        _transactions.Where(t => t.Type == InvoiceTransactionType.Payment).Sum(t => t.Amount)
        - _transactions.Where(t => t.Type == InvoiceTransactionType.Refund).Sum(t => t.Amount);

    /// <summary>
    /// Gets what is still owed on an open (Unpaid or Overdue) invoice: <see cref="Total"/> (already
    /// net of credit) less <see cref="AmountPaid"/>, never below zero; a closed invoice owes nothing.
    /// Every path that settles the invoice charges this, not the total, so a payment recorded
    /// earlier is never charged again.
    /// </summary>
    public decimal AmountDue => IsOpen ? Math.Max(0m, Total - AmountPaid) : 0m;

    /// <summary>Gets whether the invoice can still take a payment (Unpaid or Overdue).</summary>
    private bool IsOpen => Status is InvoiceStatus.Unpaid or InvoiceStatus.Overdue;

    /// <summary>
    /// Gets whether the invoice is Paid and the money covers it. <see cref="AddPayment"/> marks it
    /// Paid only once its payments cover <see cref="Total"/> (already net of credit), and
    /// <see cref="MarkPaid"/> and <see cref="MarkPaidViaGateway"/> settle <see cref="AmountDue"/>,
    /// recording it when earlier payments exist, so an invoice with recorded payments is full only
    /// when they add up to <see cref="Total"/>; one paid in one go through those two is full by construction.
    /// </summary>
    public bool IsPaidInFull
    {
        get
        {
            if (Status is not InvoiceStatus.Paid)
            {
                return false;
            }

            var hasPayments = _transactions.Any(t => t.Type == InvoiceTransactionType.Payment);
            return !hasPayments || AmountPaid >= Total;
        }
    }

    /// <summary>Appends a renewal line under the same editability rule as <see cref="AddItem"/>.</summary>
    /// <param name="item">The renewal line.</param>
    /// <exception cref="DomainRuleException">The invoice is no longer editable.</exception>
    private void AddRenewal(InvoiceItem item)
    {
        if (Status is not (InvoiceStatus.Draft or InvoiceStatus.Unpaid))
        {
            throw new DomainRuleException(
                BillingErrors.InvoiceNotEditable, $"Cannot add items to an invoice with status {Status}.");
        }

        _items.Add(item);
        RecalculateTotals();
    }

    /// <summary>
    /// Removes a line item by ID and recalculates totals.
    /// </summary>
    /// <param name="itemId">Id of the line item to remove.</param>
    public void RemoveItem(int itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainRuleException(BillingErrors.InvoiceItemNotFound, $"Invoice item {itemId} not found.");
        _items.Remove(item);
        RecalculateTotals();
    }

    /// <summary>
    /// Updates an existing line item and recalculates totals.
    /// </summary>
    /// <param name="itemId">Id of the line item to change.</param>
    /// <param name="description">New line text.</param>
    /// <param name="unitPrice">New price of one unit, in the invoice currency.</param>
    /// <param name="quantity">New number of units.</param>
    public void UpdateItem(int itemId, string description, decimal unitPrice, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainRuleException(BillingErrors.InvoiceItemNotFound, $"Invoice item {itemId} not found.");
        item.Update(description, unitPrice, quantity);
        RecalculateTotals();
    }

    /// <summary>
    /// Records a payment against this invoice. The invoice becomes Paid only once the recorded
    /// payments cover <see cref="Total"/>; until then it stays Unpaid or Overdue and takes further
    /// payments. A payment above <see cref="AmountDue"/> still settles the invoice; the excess stays
    /// on the recorded transaction for staff to settle, and is not turned into credit here.
    /// </summary>
    /// <param name="date">When the payment was made.</param>
    /// <param name="gateway">Name of the payment gateway or method.</param>
    /// <param name="transactionId">Gateway reference of the payment.</param>
    /// <param name="amount">Amount paid, in the invoice currency.</param>
    /// <param name="fees">Gateway fees charged on the payment.</param>
    /// <param name="notes">Free-form note, or null.</param>
    /// <exception cref="DomainRuleException">The invoice is not Unpaid or Overdue.</exception>
    public void AddPayment(DateTimeOffset date, string gateway, string transactionId, decimal amount, decimal fees, string? notes)
    {
        EnsurePayable();

        var tx = InvoiceTransaction.Create(Id, date, gateway, transactionId, amount, InvoiceTransactionType.Payment, fees, notes);
        _transactions.Add(tx);
        SettleIfCovered(transactionId, date, gateway);
    }

    /// <summary>
    /// Moves the invoice's creation and issue dates to an earlier moment, for invoices imported or
    /// seeded from history rather than raised now.
    /// </summary>
    /// <param name="createdAt">The moment the invoice was really raised (UTC).</param>
    public void Backdate(DateTimeOffset createdAt)
    {
        CreatedAt = createdAt;
        InvoiceDate = createdAt;
    }

    /// <summary>
    /// Updates invoice options (issue date, due date, payment method, tax rate).
    /// </summary>
    /// <param name="invoiceDate">Issue date.</param>
    /// <param name="dueDate">Date payment is due.</param>
    /// <param name="paymentMethod">Gateway module chosen for the invoice, or null.</param>
    /// <param name="taxRate">Tax percentage applied to the subtotal.</param>
    public void UpdateOptions(DateTimeOffset invoiceDate, DateTimeOffset dueDate, string? paymentMethod, decimal taxRate)
    {
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        PaymentMethod = paymentMethod;
        TaxRate = taxRate;
        RecalculateTotals();
    }

    /// <summary>
    /// Applies a credit amount to the invoice, reducing the total owed.
    /// </summary>
    /// <param name="amount">Credit to apply, in the invoice currency.</param>
    public void ApplyCredit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Credit amount must be positive.", nameof(amount));
        }

        Credit += amount;
        RecalculateTotals();
    }

    /// <summary>
    /// Removes a credit amount from the invoice.
    /// </summary>
    /// <param name="amount">Credit to take back, in the invoice currency.</param>
    public void RemoveCredit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be positive.", nameof(amount));
        }

        Credit = Math.Max(0, Credit - amount);
        RecalculateTotals();
    }

    /// <summary>
    /// Updates or clears the invoice notes.
    /// </summary>
    /// <param name="notes">The new note, or null to clear it.</param>
    public void UpdateNotes(string? notes) => Notes = notes;

    /// <summary>
    /// Records a successful payment made through a path other than the currently active
    /// hosted-gateway session (manual admin recording, card gateway, legacy import, etc.) and raises
    /// <see cref="PaymentReceivedEvent"/>. Clears any stored gateway session fields — the
    /// invariant maintained across the aggregate is that once an invoice is paid, its gateway
    /// session fields describe the gateway that actually took the money, or are null. Since this
    /// path did not use the stored session, any stored session it might overwrite is stale and
    /// must not be mistaken later for "the gateway that paid" by refund/reconciliation code. Use
    /// <see cref="MarkPaidViaGateway"/> instead when completing the invoice's own active session.
    /// </summary>
    /// <param name="gatewayTransactionId">Reference of the payment, or the admin-supplied reference for a manual one.</param>
    /// <param name="gateway">The gateway or method that took the money, recorded on the remainder's transaction.</param>
    public void MarkPaid(string gatewayTransactionId, string? gateway = null) =>
        MarkPaidAt(gatewayTransactionId, DateTimeOffset.UtcNow, gateway ?? PaymentMethod ?? UnknownGateway);

    /// <summary>Marks the invoice paid at a given moment, clears the gateway session and raises <see cref="PaymentReceivedEvent"/>.</summary>
    /// <param name="gatewayTransactionId">Reference of the payment.</param>
    /// <param name="paidAt">When the payment was received (UTC).</param>
    /// <param name="gateway">The gateway or method that took the money.</param>
    /// <exception cref="DomainRuleException">The invoice is not Unpaid or Overdue.</exception>
    private void MarkPaidAt(string gatewayTransactionId, DateTimeOffset paidAt, string gateway) =>
        Settle(gatewayTransactionId, paidAt, gateway, clearGatewaySession: true);

    /// <summary>
    /// Records a successful payment made through the invoice's own currently active
    /// hosted-gateway session and raises <see cref="PaymentReceivedEvent"/>. Unlike
    /// <see cref="MarkPaid"/>, this retains <see cref="GatewayModule"/>, <see cref="GatewayOrderId"/>
    /// and <see cref="GatewayStartedAt"/> so later refund and reconciliation code can identify
    /// which gateway actually took the money.
    /// </summary>
    /// <param name="gatewayTransactionId">The gateway's transaction reference for the completed payment.</param>
    public void MarkPaidViaGateway(string gatewayTransactionId) =>
        Settle(gatewayTransactionId, DateTimeOffset.UtcNow, GatewayModule ?? UnknownGateway, clearGatewaySession: false);

    /// <summary>
    /// Marks the invoice Paid and raises <see cref="PaymentReceivedEvent"/>: the one Paid transition
    /// every settling path goes through.
    /// </summary>
    /// <param name="gatewayTransactionId">Reference of the payment.</param>
    /// <param name="paidAt">When the payment was received (UTC).</param>
    /// <param name="gateway">The gateway or method that took the money.</param>
    /// <param name="clearGatewaySession">
    /// Whether to drop the stored gateway session: yes unless the invoice's own session paid it,
    /// whose fields refund and reconciliation code still need.
    /// </param>
    /// <exception cref="DomainRuleException">The invoice is not Unpaid or Overdue.</exception>
    private void Settle(string gatewayTransactionId, DateTimeOffset paidAt, string gateway, bool clearGatewaySession)
    {
        EnsurePayable();
        RecordRemainder(paidAt, gateway, gatewayTransactionId);

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;
        GatewayTransactionId = gatewayTransactionId;
        if (clearGatewaySession)
        {
            ClearGatewaySession();
        }

        AddDomainEvent(new PaymentReceivedEvent(Id, ClientId, Total, gatewayTransactionId));
    }

    /// <summary>
    /// Settles an open invoice whose recorded money now covers its total: after a payment, or after
    /// a credit or line change lowered the total under what was already paid.
    /// </summary>
    /// <param name="transactionId">Reference recorded as the paying transaction.</param>
    /// <param name="paidAt">When it became covered.</param>
    /// <param name="gateway">The gateway or method of the covering payment.</param>
    private void SettleIfCovered(string transactionId, DateTimeOffset paidAt, string gateway)
    {
        if (IsOpen && AmountPaid > 0m && AmountPaid >= Total)
        {
            Settle(transactionId, paidAt, gateway, clearGatewaySession: true);
        }
    }

    /// <summary>
    /// Settles an open invoice whose recorded money already covers its total, crediting the latest
    /// payment with it. Two payments recorded at the same moment each see only themselves and can
    /// leave such an invoice open; the daily renewal run calls this to close it.
    /// </summary>
    /// <returns>Whether the invoice was settled.</returns>
    public bool SettleIfCovered()
    {
        var lastPayment = _transactions.LastOrDefault(t => t.Type == InvoiceTransactionType.Payment);
        if (lastPayment is null || !IsOpen || AmountPaid < Total)
        {
            return false;
        }

        SettleIfCovered(lastPayment.TransactionId, DateTimeOffset.UtcNow, lastPayment.Gateway);
        return true;
    }

    /// <summary>Refuses a payment on an invoice that is not open.</summary>
    /// <exception cref="DomainRuleException">The invoice is not Unpaid or Overdue.</exception>
    private void EnsurePayable()
    {
        if (!IsOpen)
        {
            throw new DomainRuleException(
                BillingErrors.InvoiceNotPayable, $"Invoice cannot be paid in status {Status}.");
        }
    }

    /// <summary>
    /// When a full-charge path settles an invoice that already holds payments, records the
    /// remainder it took as a payment too, so the recorded payments cover the total and
    /// <see cref="IsPaidInFull"/> holds. An invoice paid in one go records nothing extra, as before.
    /// </summary>
    /// <param name="paidAt">When the remainder was paid.</param>
    /// <param name="gateway">The gateway or method that took it.</param>
    /// <param name="transactionId">The payment's reference.</param>
    private void RecordRemainder(DateTimeOffset paidAt, string gateway, string transactionId)
    {
        var due = AmountDue;
        if (AmountPaid > 0m && due > 0m)
        {
            _transactions.Add(InvoiceTransaction.Create(
                Id, paidAt, gateway, transactionId, due, InvoiceTransactionType.Payment, 0m, null));
        }
    }

    /// <summary>Clears the stored hosted-gateway session fields.</summary>
    private void ClearGatewaySession()
    {
        GatewayModule = null;
        GatewayOrderId = null;
        GatewayStartedAt = null;
    }

    /// <summary>
    /// Records a new hosted-gateway payment attempt. Each attempt overwrites the previous
    /// session — the gateway rejects reused order numbers, so retries always re-register.
    /// </summary>
    /// <param name="module">The payment plugin id (e.g. "bank-gateway").</param>
    /// <param name="gatewayOrderId">The gateway-side order id returned at registration.</param>
    /// <exception cref="InvalidOperationException">Thrown when the invoice is not payable.</exception>
    public void SetGatewaySession(string module, string gatewayOrderId)
    {
        if (Status is not (InvoiceStatus.Unpaid or InvoiceStatus.Overdue))
        {
            throw new InvalidOperationException($"Cannot start a gateway payment for an invoice in status {Status}.");
        }

        GatewayModule = module;
        GatewayOrderId = gatewayOrderId;
        GatewayStartedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Transitions the invoice from Unpaid to Overdue and raises <see cref="InvoiceOverdueEvent"/>.
    /// If already Overdue, this is a no-op.
    /// </summary>
    public void MarkOverdue()
    {
        if (Status == InvoiceStatus.Overdue)
        {
            return;
        }

        if (Status != InvoiceStatus.Unpaid)
        {
            throw new DomainRuleException(
                BillingErrors.InvoiceNotUnpaid,
                $"Only Unpaid invoices can be marked overdue; current status is {Status}.");
        }

        Status = InvoiceStatus.Overdue;
        AddDomainEvent(new InvoiceOverdueEvent(Id, ClientId));
    }

    /// <summary>
    /// Cancels the invoice so it will not be collected.
    /// </summary>
    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
        {
            throw new DomainRuleException(BillingErrors.InvoiceCancelNotAllowed, "Cannot cancel a paid invoice.");
        }

        Status = InvoiceStatus.Cancelled;
    }

    /// <summary>
    /// Reverses a paid invoice back to Unpaid status. Also clears any retained gateway
    /// session: an invoice paid via <see cref="MarkPaidViaGateway"/> keeps its
    /// <see cref="GatewayModule"/>/<see cref="GatewayOrderId"/>/<see cref="GatewayStartedAt"/>
    /// so refund/reconciliation code can find the paying gateway, but once an admin reverses
    /// that payment there is no longer a live payment to reconcile. Leaving the session behind
    /// would let the reconciler — which looks back 24 hours for pending gateway sessions — see
    /// what still looks like an in-flight payment, re-query the gateway, and silently re-mark
    /// the invoice Paid again, undoing the reversal.
    /// </summary>
    public void MarkUnpaid()
    {
        if (Status != InvoiceStatus.Paid)
        {
            throw new DomainRuleException(
                BillingErrors.InvoiceNotPaid,
                $"Only Paid invoices can be marked Unpaid; current status is {Status}.");
        }

        // The money is no longer held (it bounced, or was recorded by mistake): reverse it, so the
        // whole total is due again and a new payment can settle the invoice.
        var held = AmountPaid;
        if (held > 0m)
        {
            _transactions.Add(InvoiceTransaction.Create(
                Id, DateTimeOffset.UtcNow, ReversalGateway, GatewayTransactionId ?? ReversalGateway, held,
                InvoiceTransactionType.Refund, 0m, "Reversed: invoice marked unpaid"));
        }

        Status = InvoiceStatus.Unpaid;
        PaidAt = null;
        GatewayTransactionId = null;
        ClearGatewaySession();
    }

    /// <summary>
    /// Records a refund transaction against this invoice.
    /// </summary>
    /// <param name="date">When the refund was made.</param>
    /// <param name="gateway">Name of the payment gateway or method.</param>
    /// <param name="transactionId">Gateway reference of the refund.</param>
    /// <param name="amount">Amount refunded, in the invoice currency.</param>
    /// <param name="fees">Gateway fees charged on the refund.</param>
    /// <param name="notes">Free-form note, or null.</param>
    public void AddRefund(DateTimeOffset date, string gateway, string transactionId, decimal amount, decimal fees, string? notes)
    {
        var tx = InvoiceTransaction.Create(Id, date, gateway, transactionId, amount, InvoiceTransactionType.Refund, fees, notes);
        _transactions.Add(tx);
        Refund();
    }

    /// <summary>
    /// Creates a new Draft copy of this invoice (with all its line items).
    /// </summary>
    /// <remarks>
    /// The copy carries each line's <see cref="InvoiceItem.ClientServiceId"/> across. A duplicate
    /// is the same charges billed again, so a line that was for a service is still for that
    /// service; dropping the link here would quietly hide the copy from the service it belongs to.
    /// </remarks>
    /// <returns>A new Draft invoice with the same client, due date, currency and lines.</returns>
    public Invoice Duplicate()
    {
        // The same amounts in the same currency: a copy never re-labels the lines it carries.
        var copy = CreateDraft(ClientId, DueDate, Currency);
        // AddItem, never a renewal factory: a copy is not a renewal, and paying it must extend nothing.
        foreach (var item in _items)
        {
            copy.AddItem(item.Description, item.UnitPrice, item.Quantity, item.ClientServiceId);
        }
        return copy;
    }

    /// <summary>
    /// Refunds a paid invoice and marks it as Refunded.
    /// </summary>
    public void Refund()
    {
        if (Status != InvoiceStatus.Paid)
        {
            throw new DomainRuleException(
                BillingErrors.InvoiceNotPaid, $"Only Paid invoices can be refunded; current status is {Status}.");
        }

        Status = InvoiceStatus.Refunded;
        PaidAt = null;
        GatewayTransactionId = null;
    }

    /// <summary>Recomputes <see cref="SubTotal"/>, <see cref="Tax"/>, and <see cref="Total"/> from current line items.</summary>
    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.Amount);
        Tax = Math.Round(SubTotal * TaxRate / 100, 4);
        Total = SubTotal + Tax - Credit;

        // A lower total (credit, a removed or cheaper line, less tax) may now be covered by what was
        // already paid; nothing is owed then, so the invoice must not stay open and go overdue.
        SettleIfCovered();
    }
}
