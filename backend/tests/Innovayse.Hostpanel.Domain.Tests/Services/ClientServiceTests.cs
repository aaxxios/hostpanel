using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;
namespace Innovayse.Hostpanel.Domain.Tests.Services;
/// <summary>Unit tests for the <see cref="ClientService"/> aggregate.</summary>
public class ClientServiceTests
{
    /// <summary>Verifies that <see cref="ClientService.Create"/> sets Pending status and raises the created event.</summary>
    [Fact]
    public void Create_SetsPendingStatus_AndRaisesEvent()
    {
        var svc = ClientService.Create(clientId: 1, productId: 2, billingCycle: BillingCycleNames.Monthly);

        Assert.Equal(1, svc.ClientId);
        Assert.Equal(2, svc.ProductId);
        Assert.Equal(BillingCycleNames.Monthly, svc.BillingCycle);
        Assert.Equal(ServiceStatus.Pending, svc.Status);
        Assert.Single(svc.DomainEvents);
        Assert.IsType<ClientServiceCreatedEvent>(svc.DomainEvents.Single());
    }

    /// <summary>Verifies that <see cref="ClientService.Activate"/> transitions status to Active and stores the provisioning ref.</summary>
    [Fact]
    public void Activate_ChangesStatusToActive()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        svc.Activate("panel-acc-123");
        Assert.Equal(ServiceStatus.Active, svc.Status);
        Assert.Equal("panel-acc-123", svc.ProvisioningRef);
    }

    /// <summary>Verifies that <see cref="ClientService.Suspend"/> transitions status to Suspended and raises the suspended event.</summary>
    [Fact]
    public void Suspend_ChangeStatusToSuspended_AndRaisesEvent()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        svc.Activate("ref");
        svc.Suspend();
        Assert.Equal(ServiceStatus.Suspended, svc.Status);
        Assert.Contains(svc.DomainEvents, e => e is ClientServiceSuspendedEvent);
    }

    /// <summary>
    /// A caller that already updated the hosting account raises no event asking the panel to do it
    /// again, for suspend, terminate and unsuspend alike; the platform-level event still fires.
    /// </summary>
    [Fact]
    public void HostingAlreadyUpdated_RaisesNoProviderEvent()
    {
        var suspended = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        suspended.Activate("ref");
        suspended.Suspend("overdue", hostingAlreadyUpdated: true);
        Assert.Contains(suspended.DomainEvents, e => e is ClientServiceSuspendedEvent);
        Assert.DoesNotContain(suspended.DomainEvents, e => e is ServiceSuspendedEvent);

        suspended.ClearDomainEvents();
        suspended.Unsuspend(hostingAlreadyUpdated: true);
        Assert.Contains(suspended.DomainEvents, e => e is ClientServiceUnsuspendedEvent);
        Assert.DoesNotContain(suspended.DomainEvents, e => e is ServiceUnsuspendedEvent);

        var terminated = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        terminated.Activate("ref");
        terminated.Terminate("gone", hostingAlreadyUpdated: true);
        Assert.Contains(terminated.DomainEvents, e => e is ClientServiceTerminatedEvent);
        Assert.DoesNotContain(terminated.DomainEvents, e => e is ServiceTerminatedEvent);
    }

    /// <summary>Verifies that <see cref="ClientService.Terminate"/> transitions status to Terminated and raises the terminated event.</summary>
    [Fact]
    public void Terminate_ChangesStatusToTerminated_AndRaisesEvent()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        svc.Activate("ref");
        svc.Terminate();
        Assert.Equal(ServiceStatus.Terminated, svc.Status);
        Assert.Contains(svc.DomainEvents, e => e is ClientServiceTerminatedEvent);
    }

    /// <summary>Verifies that <see cref="ClientService.Activate"/> sets NextRenewalAt for monthly and annual billing cycles.</summary>
    [Fact]
    public void Activate_SetsNextRenewalAt_ForMonthlyAndAnnual()
    {
        var monthlySvc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        monthlySvc.Activate("ref-m");
        Assert.NotNull(monthlySvc.NextRenewalAt);
        Assert.True(monthlySvc.NextRenewalAt > DateTimeOffset.UtcNow);

        var annualSvc = ClientService.Create(1, 2, BillingCycleNames.Annual);
        annualSvc.Activate("ref-a");
        Assert.NotNull(annualSvc.NextRenewalAt);
        Assert.True(annualSvc.NextRenewalAt > monthlySvc.NextRenewalAt);
    }

    /// <summary>Verifies that <see cref="ClientService.Suspend"/> sets SuspendedAt timestamp.</summary>
    [Fact]
    public void Suspend_SetsSuspendedAt()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        svc.Activate("ref");

        var before = DateTimeOffset.UtcNow;
        svc.Suspend();
        var after = DateTimeOffset.UtcNow;

        Assert.NotNull(svc.SuspendedAt);
        Assert.True(svc.SuspendedAt >= before && svc.SuspendedAt <= after);
    }

    /// <summary>Verifies that <see cref="ClientService.Unsuspend"/> clears SuspendedAt.</summary>
    [Fact]
    public void Unsuspend_ClearsSuspendedAt()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        svc.Activate("ref");
        svc.Suspend();
        Assert.NotNull(svc.SuspendedAt);

        svc.Unsuspend();

        Assert.Null(svc.SuspendedAt);
        Assert.Equal(ServiceStatus.Active, svc.Status);
    }

    /// <summary>Suspend on a Pending service throws <see cref="DomainRuleException"/> with <c>SERVICE_NOT_ACTIVE</c>.</summary>
    [Fact]
    public void Suspend_WhenNotActive_ThrowsWithServiceNotActive()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);

        var ex = Assert.Throws<DomainRuleException>(() => svc.Suspend());

        Assert.Equal(ServicesErrors.ServiceNotActive, ex.Code);
    }

    /// <summary>Terminate on an already-terminated service throws <see cref="DomainRuleException"/> with <c>SERVICE_TERMINATE_NOT_ALLOWED</c>.</summary>
    [Fact]
    public void Terminate_WhenAlreadyTerminated_ThrowsWithServiceTerminateNotAllowed()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        svc.Activate("ref");
        svc.Terminate();

        var ex = Assert.Throws<DomainRuleException>(() => svc.Terminate());

        Assert.Equal(ServicesErrors.ServiceTerminateNotAllowed, ex.Code);
    }

    /// <summary>Unsuspend on an Active service throws <see cref="DomainRuleException"/> with <c>SERVICE_NOT_SUSPENDED</c>.</summary>
    [Fact]
    public void Unsuspend_WhenNotSuspended_ThrowsWithServiceNotSuspended()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        svc.Activate("ref");

        var ex = Assert.Throws<DomainRuleException>(() => svc.Unsuspend());

        Assert.Equal(ServicesErrors.ServiceNotSuspended, ex.Code);
    }

    /// <summary>Verifies that <see cref="ClientService.Backdate"/> moves the creation date and keeps the status.</summary>
    [Fact]
    public void Backdate_MovesTheCreationDateOnly()
    {
        var svc = ClientService.Create(1, 2, BillingCycleNames.Monthly);
        var ordered = new DateTimeOffset(2026, 1, 5, 9, 0, 0, TimeSpan.Zero);

        svc.Backdate(ordered);

        Assert.Equal(ordered, svc.CreatedAt);
        Assert.Equal(ServiceStatus.Pending, svc.Status);
    }
}
