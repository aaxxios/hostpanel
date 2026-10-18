using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>
/// Publishes the domain events entities raised, once the save that made them true has committed.
/// </summary>
/// <remarks>
/// <para>
/// Entities only record events (<c>AddDomainEvent</c>); nothing else delivers them. This interceptor
/// is the delivery: after a successful <c>SaveChangesAsync</c> it walks the tracked aggregates,
/// takes and clears each one's events, and hands every event to Wolverine, whose local queues run
/// the matching handlers asynchronously in their own scopes. Publishing after the commit means a
/// handler never observes state that was rolled back, and a handler failure never fails the
/// request that already succeeded.
/// </para>
/// <para>
/// Events are read after the save, not before, because a creation event names the entity's own
/// id and the database assigns it on insert. A save that throws leaves the events on the entities,
/// so a retried save still delivers them exactly once. The synchronous <c>SaveChanges</c> is
/// deliberately not intercepted: publishing is asynchronous and nothing in the product saves
/// synchronously; its events are delivered by the next asynchronous save of the same context.
/// </para>
/// </remarks>
public sealed class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    /// <summary>The scope's provider; the bus is resolved lazily so building the context never builds the bus.</summary>
    private readonly IServiceProvider _services;

    /// <summary>Tells whether this scope asked for its events to be discarded.</summary>
    private readonly IDomainEventGate _gate;

    /// <summary>Records a publish that failed after the data was already committed.</summary>
    private readonly ILogger<DomainEventDispatchInterceptor> _logger;

    /// <summary>Initializes a new instance of the <see cref="DomainEventDispatchInterceptor"/> class.</summary>
    /// <param name="services">The scope's provider; the bus is resolved lazily so building the context never builds the bus.</param>
    /// <param name="gate">Tells whether this scope asked for its events to be discarded.</param>
    /// <param name="logger">Records a publish that failed after the data was already committed.</param>
    public DomainEventDispatchInterceptor(IServiceProvider services, IDomainEventGate gate, ILogger<DomainEventDispatchInterceptor> logger)
    {
        _services = services;
        _gate = gate;
        _logger = logger;
    }

    /// <inheritdoc/>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchAsync(eventData.Context);
        }

        return result;
    }

    /// <summary>Takes every tracked aggregate's events, clears them, and publishes them in raise order.</summary>
    /// <param name="context">The context that just saved.</param>
    /// <returns>A task that completes once every event has been handed to the bus.</returns>
    private async Task DispatchAsync(DbContext context)
    {
        var pending = new List<IDomainEvent>();
        foreach (var entity in context.ChangeTracker.Entries<BaseEntity>().Select(e => e.Entity))
        {
            var raised = entity.DomainEvents;
            if (raised.Count == 0)
            {
                continue;
            }

            pending.AddRange(raised);

            // Cleared before publishing, so an event can never be delivered twice, however the
            // handlers behave.
            entity.ClearDomainEvents();
        }

        if (pending.Count == 0 || _gate.IsSuppressed)
        {
            return;
        }

        var bus = _services.GetRequiredService<IMessageBus>();
        foreach (var domainEvent in pending)
        {
            try
            {
                // The runtime type picks the route, so each handler subscribed to the concrete
                // event runs; the data is already committed, hence log-and-continue, not rethrow.
                await bus.PublishAsync((object)domainEvent);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex, "Publishing domain event {EventType} failed after the save committed", domainEvent.GetType().Name);
            }
        }
    }
}
