namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>A service whose billing cycle has ended while it was set to terminate at the end of it.</summary>
/// <param name="ServiceId">The service primary key.</param>
/// <param name="Reason">Why it ends, as staff or the cancellation request recorded it; or <see langword="null"/> when none was given.</param>
public sealed record EndOfCycleTerminationDto(int ServiceId, string? Reason);
