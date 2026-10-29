using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A save interceptor that throws, before anything reaches the database, whenever the pending
/// change set matches a predicate. It stands in for a crash or a database error at one chosen
/// save, so a test can prove what a job leaves behind when that save never happens.
/// </summary>
/// <param name="refuses">Tells whether the pending changes must be refused.</param>
internal sealed class FailingSaveInterceptor(Func<ChangeTracker, bool> refuses) : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db && refuses(db.ChangeTracker))
        {
            throw new InvalidOperationException("Simulated failure of one save.");
        }

        return ValueTask.FromResult(result);
    }
}
