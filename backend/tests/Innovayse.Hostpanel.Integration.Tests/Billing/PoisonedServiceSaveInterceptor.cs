using Innovayse.Hostpanel.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>A save interceptor that throws when a modified poisoned service is about to be saved.</summary>
/// <param name="poisoned">The poisoned service.</param>
internal sealed class PoisonedServiceSaveInterceptor(PoisonedService poisoned) : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (poisoned.ServiceId != 0 && eventData.Context is { } db
            && db.ChangeTracker.Entries<ClientService>().Any(e => e.State == EntityState.Modified && e.Entity.Id == poisoned.ServiceId))
        {
            throw new InvalidOperationException("Simulated failure for the poisoned service.");
        }

        return ValueTask.FromResult(result);
    }
}
