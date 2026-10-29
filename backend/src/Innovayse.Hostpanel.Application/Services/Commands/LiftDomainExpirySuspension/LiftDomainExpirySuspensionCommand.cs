namespace Innovayse.Hostpanel.Application.Services.Commands.LiftDomainExpirySuspension;

/// <summary>
/// Lifts the suspension a domain's expiry put on its linked service, once the domain is renewed.
/// A suspension with any other cause is left in place.
/// </summary>
/// <param name="ServiceId">The service linked to the renewed domain.</param>
public sealed record LiftDomainExpirySuspensionCommand(int ServiceId);
