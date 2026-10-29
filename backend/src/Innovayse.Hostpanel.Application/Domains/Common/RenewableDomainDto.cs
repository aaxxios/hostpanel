namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>An active, auto-renewing domain that expires inside a window, as recurring billing reads it.</summary>
/// <param name="Id">The domain.</param>
/// <param name="ClientId">The client who owns it.</param>
/// <param name="Name">The fully-qualified name.</param>
/// <param name="ExpiresAt">Its current expiry, the start of the period to bill.</param>
/// <param name="RegistrationPeriod">The period it is registered for, in years.</param>
public sealed record RenewableDomainDto(int Id, int ClientId, string Name, DateTimeOffset ExpiresAt, int RegistrationPeriod);
