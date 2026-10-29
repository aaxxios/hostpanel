using Innovayse.Hostpanel.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innovayse.Hostpanel.Integration.Tests.Notifications;

/// <summary>
/// A save interceptor that holds every save adding email templates of one slug until the given
/// number of saves has arrived. Concurrent seeders therefore all find the versions missing before
/// any of them writes, which is the race the unique index on slug and language has to settle.
/// </summary>
/// <param name="slug">The template slug whose inserting saves are held.</param>
/// <param name="parties">How many saves must arrive before all of them are let through.</param>
internal sealed class TemplateSeedRendezvousInterceptor(string slug, int parties) : SaveChangesInterceptor
{
    /// <summary>How long a held save waits for the others before the test fails instead of hanging.</summary>
    private static readonly TimeSpan MaxWait = TimeSpan.FromSeconds(30);

    /// <summary>Completed once the last expected save has arrived.</summary>
    private readonly TaskCompletionSource _allArrived = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>How many inserting saves have arrived so far.</summary>
    private int _arrived;

    /// <summary>How many inserting saves have arrived so far.</summary>
    public int Arrived => Volatile.Read(ref _arrived);

    /// <inheritdoc/>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db
            && db.ChangeTracker.Entries<EmailTemplate>().Any(e => e.State == EntityState.Added && e.Entity.Slug == slug))
        {
            if (Interlocked.Increment(ref _arrived) >= parties)
            {
                _allArrived.TrySetResult();
            }

            await _allArrived.Task.WaitAsync(MaxWait, cancellationToken);
        }

        return result;
    }
}
