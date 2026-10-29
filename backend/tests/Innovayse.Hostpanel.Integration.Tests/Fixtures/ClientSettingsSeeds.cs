using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Flips a seeded client's account switches through the aggregate's own
/// <c>UpdateSettings</c>, leaving every switch not named as it was.
/// </summary>
internal static class ClientSettingsSeeds
{
    /// <summary>Writes the named switches on the client and keeps the others.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="clientId">The client to change.</param>
    /// <param name="overdueNotices">The new payment reminder switch, or null to keep it.</param>
    /// <param name="taxExempt">The new tax exemption, or null to keep it.</param>
    /// <param name="disableCcProcessing">The new card processing switch, or null to keep it.</param>
    /// <param name="allowSso">The new panel single sign-on switch, or null to keep it.</param>
    /// <returns>A task that completes once the switches are saved.</returns>
    public static async Task SetAsync(
        IntegrationTestFactory factory,
        int clientId,
        bool? overdueNotices = null,
        bool? taxExempt = null,
        bool? disableCcProcessing = null,
        bool? allowSso = null)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var client = await context.Clients.FirstAsync(c => c.Id == clientId, CancellationToken.None);

        client.UpdateSettings(
            overdueNotices ?? client.OverdueNotices,
            taxExempt ?? client.TaxExempt,
            client.SeparateInvoices,
            disableCcProcessing ?? client.DisableCcProcessing,
            client.MarketingOptIn,
            client.StatusUpdate,
            allowSso ?? client.AllowSso);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
