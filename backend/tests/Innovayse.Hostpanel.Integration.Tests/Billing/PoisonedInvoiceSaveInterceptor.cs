using Innovayse.Hostpanel.Domain.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>A save interceptor that throws when a reminder row for the poisoned invoice is about to be saved.</summary>
/// <param name="poisoned">The poisoned invoice.</param>
internal sealed class PoisonedInvoiceSaveInterceptor(PoisonedInvoice poisoned) : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db
            && db.ChangeTracker.Entries<InvoiceReminder>().Any(e => e.State == EntityState.Added && e.Entity.InvoiceId == poisoned.InvoiceId))
        {
            throw new InvalidOperationException("Simulated failure for the poisoned invoice.");
        }

        return ValueTask.FromResult(result);
    }
}
