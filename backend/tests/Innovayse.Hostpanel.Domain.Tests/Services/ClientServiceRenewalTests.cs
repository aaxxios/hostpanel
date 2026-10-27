using FluentAssertions;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Services;

/// <summary>Renewal and suspension state on <see cref="ClientService"/>.</summary>
public sealed class ClientServiceRenewalTests
{
    /// <summary>Builds an active service with the given cycle.</summary>
    /// <param name="cycle">The stored billing-cycle spelling.</param>
    /// <returns>The activated service.</returns>
    private static ClientService Active(string cycle)
    {
        var service = ClientService.Create(1, 1, cycle);
        service.Activate("ref");
        return service;
    }

    /// <summary>"annually" activates on a yearly renewal -- it used to fall through to monthly.</summary>
    [Fact]
    public void Activate_Annually_RenewsInAYear()
    {
        var service = Active("annually");

        service.NextRenewalAt.Should().Be(service.CreatedAt.AddYears(1));
    }

    /// <summary>An unknown cycle activates with no renewal date instead of a guessed monthly one.</summary>
    [Fact]
    public void Activate_UnknownCycle_HasNoRenewalDate()
    {
        Active("onetime").NextRenewalAt.Should().BeNull();
    }

    /// <summary>Auto-renew is on for a new service.</summary>
    [Fact]
    public void Create_AutoRenewDefaultsOn()
    {
        ClientService.Create(1, 1, "monthly").AutoRenew.Should().BeTrue();
    }

    /// <summary>Extending moves the date forward and never back.</summary>
    [Fact]
    public void ExtendRenewalTo_TakesTheLaterDate()
    {
        var service = Active("monthly");
        var later = service.NextRenewalAt!.Value.AddMonths(1);

        service.ExtendRenewalTo(later);
        service.ExtendRenewalTo(later.AddMonths(-3));

        service.NextRenewalAt.Should().Be(later);
    }

    /// <summary>The suspension source is recorded and cleared on unsuspend; manual is the default.</summary>
    [Fact]
    public void Suspend_RecordsSource_UnsuspendClearsIt()
    {
        var service = Active("monthly");

        service.Suspend("unpaid", hostingAlreadyUpdated: true, SuspensionSource.Overdue);
        service.SuspensionSource.Should().Be(SuspensionSource.Overdue);

        service.Unsuspend(hostingAlreadyUpdated: true);
        service.SuspensionSource.Should().BeNull();

        service.Suspend();
        service.SuspensionSource.Should().Be(SuspensionSource.Manual);
    }
}
