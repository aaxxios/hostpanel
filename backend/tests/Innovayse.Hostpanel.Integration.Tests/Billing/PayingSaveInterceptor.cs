using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>A save interceptor that, before the trigger service's suspension is saved, first marks another invoice paid.</summary>
/// <param name="race">The trigger and the invoice to pay.</param>
internal sealed class PayingSaveInterceptor(PaymentRace race) : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (!race.Fired && race.TriggerServiceId != 0 && eventData.Context is { } db
            && db.ChangeTracker.Entries<ClientService>().Any(e => e.State == EntityState.Modified && e.Entity.Id == race.TriggerServiceId))
        {
            // Written the way a payment ends up in the row; the event path is irrelevant here.
            await db.Set<Invoice>()
                .Where(i => i.Id == race.InvoiceToPay)
                .ExecuteUpdateAsync(set => set.SetProperty(i => i.Status, InvoiceStatus.Paid), cancellationToken);
            race.Fired = true;
        }

        return result;
    }
}
