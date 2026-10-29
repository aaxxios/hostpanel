using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Services;

/// <summary>Renewal and suspension state on <see cref="ClientService"/>.</summary>
public sealed class ClientServiceRenewalTests
{
    /// <summary>The moment every service in these tests starts billing from.</summary>
    private static readonly DateTimeOffset BillingAnchor = new(2026, 1, 15, 9, 30, 0, TimeSpan.Zero);

    /// <summary>The fixed moment every domain method in these tests is handed as "now".</summary>
    private static readonly DateTimeOffset Now = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Builds an active service with the given cycle.</summary>
    /// <param name="cycle">The billing cycle.</param>
    /// <returns>The activated service.</returns>
    private static ClientService Active(BillingCycle cycle)
    {
        var service = ClientService.Create(1, 1, cycle, Now);
        service.Activate("ref", BillingAnchor, Now);
        return service;
    }

    /// <summary>An annual service activates on a yearly renewal, not a monthly one.</summary>
    [Fact]
    public void Activate_Annual_RenewsInAYear()
    {
        var service = Active(BillingCycle.Annual);

        service.NextRenewalAt.Should().Be(BillingAnchor.AddYears(1));
    }

    /// <summary>Auto-renew is on for a new service.</summary>
    [Fact]
    public void Create_AutoRenewDefaultsOn()
    {
        ClientService.Create(1, 1, BillingCycle.Monthly, Now).AutoRenew.Should().BeTrue();
    }

    /// <summary>Extending moves the date forward and never back.</summary>
    [Fact]
    public void ExtendRenewalTo_TakesTheLaterDate()
    {
        var service = Active(BillingCycle.Monthly);
        var later = service.NextRenewalAt!.Value.AddMonths(1);

        service.ExtendRenewalTo(later);
        service.ExtendRenewalTo(later.AddMonths(-3));

        service.NextRenewalAt.Should().Be(later);
    }

    /// <summary>The suspension source is recorded and cleared on unsuspend; manual is the default.</summary>
    [Fact]
    public void Suspend_RecordsSource_UnsuspendClearsIt()
    {
        var service = Active(BillingCycle.Monthly);

        service.Suspend(Now, "unpaid", hostingAlreadyUpdated: true, SuspensionSource.Overdue);
        service.SuspensionSource.Should().Be(SuspensionSource.Overdue);

        service.Unsuspend(Now, hostingAlreadyUpdated: true);
        service.SuspensionSource.Should().BeNull();

        service.Suspend(Now);
        service.SuspensionSource.Should().Be(SuspensionSource.Manual);
    }

    /// <summary>Activation anchors the first renewal on the moment the caller passes, not on the creation stamp.</summary>
    [Fact]
    public void Activate_Monthly_RenewsAMonthAfterTheBillingAnchor()
    {
        var service = Active(BillingCycle.Monthly);

        service.NextRenewalAt.Should().Be(BillingAnchor.AddMonths(1));
    }
}
