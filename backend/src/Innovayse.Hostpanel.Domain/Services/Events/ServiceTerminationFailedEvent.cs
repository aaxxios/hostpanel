using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Services.Events;

/// <summary>
/// An automatic termination could not remove the service's hosting account (the panel refused or
/// could not be reached). The service keeps its state, so the next run retries it; staff are told.
/// </summary>
/// <param name="ServiceId">The service that could not be terminated.</param>
/// <param name="Reason">What went wrong, for staff only.</param>
public record ServiceTerminationFailedEvent(int ServiceId, string Reason) : IDomainEvent
{
    /// <summary>When the change was made, UTC, given by whoever raised the event.</summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
