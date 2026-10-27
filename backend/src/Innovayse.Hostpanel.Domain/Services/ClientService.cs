using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;
namespace Innovayse.Hostpanel.Domain.Services;
/// <summary>
/// Represents a service instance owned by a client.
/// Links a <see cref="ClientId"/> to a product and tracks provisioning state.
/// Stored in the <c>ClientServices</c> table.
/// </summary>
public sealed class ClientService : BaseEntity, IAggregateRoot
{
    /// <summary>Gets the FK to the owning client.</summary>
    public int ClientId { get; private set; }

    /// <summary>Gets the FK to the ordered product.</summary>
    public int ProductId { get; private set; }

    /// <summary>Gets the selected billing cycle ("monthly" or "annual").</summary>
    public string BillingCycle { get; private set; } = string.Empty;

    /// <summary>Gets the current lifecycle status.</summary>
    public ServiceStatus Status { get; private set; }

    /// <summary>Gets the external provisioning reference (e.g., hosting account name).</summary>
    public string? ProvisioningRef { get; private set; }

    /// <summary>Gets the UTC timestamp when the service was ordered.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets the UTC timestamp of the next renewal date, if active.</summary>
    public DateTimeOffset? NextRenewalAt { get; private set; }

    /// <summary>Gets the linked domain name.</summary>
    public string? Domain { get; private set; }

    /// <summary>Gets the dedicated IP address.</summary>
    public string? DedicatedIp { get; private set; }

    /// <summary>Gets the hosting account username.</summary>
    public string? Username { get; private set; }

    /// <summary>Gets the hosting account password.</summary>
    public string? Password { get; private set; }

    /// <summary>Gets the quantity ordered.</summary>
    public int Quantity { get; private set; } = 1;

    /// <summary>Gets the first payment amount.</summary>
    public decimal FirstPaymentAmount { get; private set; }

    /// <summary>Gets the recurring charge amount.</summary>
    public decimal RecurringAmount { get; private set; }

    /// <summary>Gets the payment method for this service.</summary>
    public string? PaymentMethod { get; private set; }

    /// <summary>Gets the applied promotion code.</summary>
    public string? PromotionCode { get; private set; }

    /// <summary>Gets the external subscription reference.</summary>
    public string? SubscriptionId { get; private set; }

    /// <summary>Gets the UTC timestamp when the service was suspended, or <see langword="null"/> if not suspended.</summary>
    public DateTimeOffset? SuspendedAt { get; private set; }

    /// <summary>Gets when the service was terminated.</summary>
    public DateTimeOffset? TerminatedAt { get; private set; }

    /// <summary>Gets whether auto-suspend is overridden.</summary>
    public bool OverrideAutoSuspend { get; private set; }

    /// <summary>Gets the date until which auto-suspend is blocked.</summary>
    public DateTimeOffset? SuspendUntil { get; private set; }

    /// <summary>Gets whether to auto-terminate at end of billing cycle.</summary>
    public bool AutoTerminateEndOfCycle { get; private set; }

    /// <summary>Gets the reason for auto-termination.</summary>
    public string? AutoTerminateReason { get; private set; }

    /// <summary>Gets the FK to the provisioning server, or null if not assigned.</summary>
    public int? ServerId { get; private set; }

    /// <summary>Gets the internal admin notes.</summary>
    public string? AdminNotes { get; private set; }

    /// <summary>
    /// Gets whether a renewal invoice is raised automatically ahead of <see cref="NextRenewalAt"/>.
    /// <see langword="false"/> means staff bill the service by hand; it neither suspends nor terminates it.
    /// </summary>
    public bool AutoRenew { get; private set; } = true;

    /// <summary>Gets why the service is suspended, or <see langword="null"/> when it is not (or the reason predates this column).</summary>
    public SuspensionSource? SuspensionSource { get; private set; }

    /// <summary>EF Core parameterless constructor — do not call directly.</summary>
    private ClientService() { }

    /// <summary>
    /// Creates a new pending client service and raises <see cref="ClientServiceCreatedEvent"/>.
    /// </summary>
    /// <param name="clientId">FK to the client placing the order.</param>
    /// <param name="productId">FK to the product being ordered.</param>
    /// <param name="billingCycle">Billing cycle: "monthly" or "annual".</param>
    /// <returns>A new pending <see cref="ClientService"/>.</returns>
    public static ClientService Create(int clientId, int productId, string billingCycle)
    {
        var svc = new ClientService
        {
            ClientId = clientId,
            ProductId = productId,
            BillingCycle = billingCycle,
            Status = ServiceStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        svc.AddDomainEvent(() => new ClientServiceCreatedEvent(svc.Id, clientId, productId));
        return svc;
    }

    /// <summary>Moves the creation date to the moment the service was really ordered, for services seeded from history.</summary>
    /// <param name="createdAt">The moment the service was ordered (UTC).</param>
    public void Backdate(DateTimeOffset createdAt) => CreatedAt = createdAt;

    /// <summary>
    /// Marks the service as active after successful provisioning.
    /// Sets the provisioning reference and calculates the first renewal date.
    /// </summary>
    /// <param name="provisioningRef">External reference from the provisioning provider.</param>
    /// <exception cref="InvalidOperationException">Thrown when the service is not in Pending status.</exception>
    public void Activate(string provisioningRef)
    {
        if (Status is not ServiceStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot activate a service with status {Status}. Only Pending services can be activated.");
        }

        Status = ServiceStatus.Active;
        ProvisioningRef = provisioningRef;

        // Through RenewalPeriod, which knows every stored spelling: comparing to "annual" alone
        // renewed an "annually" service monthly at its annual price. An unknown cycle (free,
        // one-time) has no renewal date rather than a guessed monthly one.
        NextRenewalAt = RenewalPeriod.EndOf(CreatedAt, BillingCycle, CreatedAt.Day);
        AddDomainEvent(new ServiceProvisionedEvent(Id, ClientId, provisioningRef));
    }

    /// <summary>
    /// Records the server the account was provisioned on and activates the service, as one
    /// transition.
    /// </summary>
    /// <remarks>
    /// <see cref="ServerId"/> and activation change together. Every later operation on a live
    /// service — suspend, unsuspend, terminate, control-panel single sign-on — resolves its
    /// provisioning provider from <see cref="ServerId"/> and gives up silently when it is null,
    /// so a service that reached <see cref="ServiceStatus.Active"/> without a server can never be
    /// acted on again. Activating and assigning through two separate calls is what let that
    /// happen, which is why this is one method rather than a public setter.
    /// </remarks>
    /// <param name="serverId">Identifier of the server the hosting account now lives on.</param>
    /// <param name="provisioningRef">External reference from the provisioning provider.</param>
    /// <exception cref="InvalidOperationException">Thrown when the service is not in Pending status.</exception>
    public void ActivateOn(int serverId, string provisioningRef)
    {
        // Activate first: it is the guarded half, and a refused activation must leave the
        // server assignment untouched.
        Activate(provisioningRef);
        ServerId = serverId;
    }

    /// <summary>
    /// Suspends the service and raises <see cref="ClientServiceSuspendedEvent"/>
    /// and <see cref="ServiceSuspendedEvent"/>.
    /// </summary>
    /// <param name="reason">Human-readable reason for the suspension.</param>
    /// <param name="hostingAlreadyUpdated">
    /// The caller has already suspended the hosting account itself (and answered its failure), so
    /// <see cref="ServiceSuspendedEvent"/> -- the request for the panel to do it -- is not raised
    /// and the account is not suspended a second time.
    /// </param>
    /// <param name="source">Why it is suspended; decides whether a later payment may lift it.</param>
    /// <exception cref="DomainRuleException">Thrown when the service is not in Active status.</exception>
    public void Suspend(string reason = "", bool hostingAlreadyUpdated = false, SuspensionSource source = Enums.SuspensionSource.Manual)
    {
        if (Status is not ServiceStatus.Active)
        {
            throw new DomainRuleException(
                ServicesErrors.ServiceNotActive,
                $"Cannot suspend a service with status {Status}. Only Active services can be suspended.");
        }

        Status = ServiceStatus.Suspended;
        SuspendedAt = DateTimeOffset.UtcNow;
        SuspensionSource = source;
        AddDomainEvent(new ClientServiceSuspendedEvent(Id, ClientId));
        if (!hostingAlreadyUpdated)
        {
            AddDomainEvent(new ServiceSuspendedEvent(Id, ClientId, reason));
        }
    }

    /// <summary>
    /// Terminates the service and raises <see cref="ClientServiceTerminatedEvent"/>
    /// and <see cref="ServiceTerminatedEvent"/>.
    /// </summary>
    /// <param name="reason">Human-readable reason for the termination.</param>
    /// <param name="hostingAlreadyUpdated">
    /// The caller has already terminated the hosting account itself, so
    /// <see cref="ServiceTerminatedEvent"/> is not raised and the account is not terminated twice.
    /// </param>
    /// <exception cref="DomainRuleException">Thrown when the service is already terminated.</exception>
    public void Terminate(string reason = "", bool hostingAlreadyUpdated = false)
    {
        if (Status is ServiceStatus.Terminated)
        {
            throw new DomainRuleException(ServicesErrors.ServiceTerminateNotAllowed, "Service is already terminated.");
        }

        Status = ServiceStatus.Terminated;
        TerminatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ClientServiceTerminatedEvent(Id, ClientId));
        if (!hostingAlreadyUpdated)
        {
            AddDomainEvent(new ServiceTerminatedEvent(Id, ClientId, reason));
        }
    }

    /// <summary>
    /// Re-activates a previously suspended service, restoring it to active status.
    /// </summary>
    /// <param name="hostingAlreadyUpdated">
    /// The caller has already unsuspended the hosting account itself, so
    /// <see cref="Provisioning.Events.ServiceUnsuspendedEvent"/> is not raised and the account is not
    /// unsuspended twice.
    /// </param>
    /// <exception cref="DomainRuleException">Thrown when the service is not in Suspended status.</exception>
    public void Unsuspend(bool hostingAlreadyUpdated = false)
    {
        if (Status is not ServiceStatus.Suspended)
        {
            throw new DomainRuleException(
                ServicesErrors.ServiceNotSuspended,
                $"Cannot unsuspend a service with status {Status}. Only Suspended services can be unsuspended.");
        }

        Status = ServiceStatus.Active;
        SuspendedAt = null;
        SuspensionSource = null;
        if (!hostingAlreadyUpdated)
        {
            AddDomainEvent(new Provisioning.Events.ServiceUnsuspendedEvent(Id, ClientId));
        }
    }

    /// <summary>Turns automatic renewal invoicing on or off.</summary>
    /// <param name="value">The new flag.</param>
    public void SetAutoRenew(bool value) => AutoRenew = value;

    /// <summary>
    /// Records that the period ending at <paramref name="periodEnd"/> is paid: the renewal date
    /// becomes the later of its current value and <paramref name="periodEnd"/>. Never moves a date
    /// back, so a replayed payment or a date staff already pushed further is left alone.
    /// </summary>
    /// <param name="periodEnd">End of the paid period.</param>
    public void ExtendRenewalTo(DateTimeOffset periodEnd)
    {
        if (NextRenewalAt is null || periodEnd > NextRenewalAt)
        {
            NextRenewalAt = periodEnd;
        }
    }

    /// <summary>
    /// Updates the editable service fields.
    /// </summary>
    /// <param name="domain">Linked domain name.</param>
    /// <param name="dedicatedIp">Dedicated IP address.</param>
    /// <param name="username">Hosting account username.</param>
    /// <param name="password">Hosting account password.</param>
    /// <param name="billingCycle">Billing cycle: "monthly" or "annual".</param>
    /// <param name="recurringAmount">Recurring charge amount.</param>
    /// <param name="paymentMethod">Payment method for this service.</param>
    /// <param name="nextRenewalAt">Next renewal date.</param>
    /// <param name="subscriptionId">External subscription reference.</param>
    /// <param name="overrideAutoSuspend">Whether auto-suspend is overridden.</param>
    /// <param name="suspendUntil">Date until which auto-suspend is blocked.</param>
    /// <param name="autoTerminateEndOfCycle">Whether to auto-terminate at end of billing cycle.</param>
    /// <param name="autoTerminateReason">Reason for auto-termination.</param>
    /// <param name="adminNotes">Internal admin notes.</param>
    /// <param name="provisioningRef">Server/provisioning reference.</param>
    /// <param name="firstPaymentAmount">First payment amount.</param>
    /// <param name="promotionCode">Promotion/coupon code.</param>
    /// <param name="terminatedAt">Termination date (admin override).</param>
    /// <param name="serverId">FK to the provisioning server (null to clear).</param>
    /// <param name="quantity">Quantity ordered.</param>
    /// <param name="productId">FK to the product (null to keep current).</param>
    public void Update(string? domain, string? dedicatedIp, string? username,
        string? password, string billingCycle, decimal recurringAmount,
        string? paymentMethod, DateTimeOffset? nextRenewalAt,
        string? subscriptionId, bool overrideAutoSuspend,
        DateTimeOffset? suspendUntil, bool autoTerminateEndOfCycle,
        string? autoTerminateReason, string? adminNotes,
        string? provisioningRef, decimal firstPaymentAmount,
        string? promotionCode, DateTimeOffset? terminatedAt,
        int? serverId, int quantity, int? productId)
    {
        Domain = domain;
        DedicatedIp = dedicatedIp;
        Username = username;
        Password = password;
        BillingCycle = billingCycle;
        RecurringAmount = recurringAmount;
        PaymentMethod = paymentMethod;
        NextRenewalAt = nextRenewalAt;
        SubscriptionId = subscriptionId;
        OverrideAutoSuspend = overrideAutoSuspend;
        SuspendUntil = suspendUntil;
        AutoTerminateEndOfCycle = autoTerminateEndOfCycle;
        AutoTerminateReason = autoTerminateReason;
        AdminNotes = adminNotes;
        ProvisioningRef = provisioningRef;
        FirstPaymentAmount = firstPaymentAmount;
        PromotionCode = promotionCode;
        TerminatedAt = terminatedAt;
        ServerId = serverId;
        Quantity = quantity;

        if (productId.HasValue)
        {
            ProductId = productId.Value;
        }
    }
}
