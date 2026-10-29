namespace Innovayse.Hostpanel.Application.Services.Commands.ExtendServiceRenewal;

/// <summary>
/// Moves a service's next renewal date to the end of a paid period. Idempotent: the date only ever
/// moves forward, so sending the same period twice changes nothing the second time.
/// </summary>
/// <param name="ServiceId">The service.</param>
/// <param name="PeriodEnd">End of the period that was paid for.</param>
public sealed record ExtendServiceRenewalCommand(int ServiceId, DateTimeOffset PeriodEnd);
