namespace Innovayse.Hostpanel.Application.Services.Queries.ListEndOfCycleTerminationsDue;

/// <summary>
/// Lists the services set to terminate at the end of their billing cycle whose renewal date is at or
/// before <paramref name="DueBy"/> and that are not terminated yet.
/// </summary>
/// <param name="DueBy">The moment the cycle must have ended by; the caller's current moment.</param>
public sealed record ListEndOfCycleTerminationsDueQuery(DateTimeOffset DueBy);
