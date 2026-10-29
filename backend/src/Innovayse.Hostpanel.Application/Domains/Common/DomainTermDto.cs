namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The dates and length of a domain's registration.</summary>
/// <param name="RegisteredAt">Domain registration date (UTC).</param>
/// <param name="ExpiresAt">Domain expiration date (UTC).</param>
/// <param name="NextDueDate">Next renewal payment due date (UTC).</param>
/// <param name="RegistrationPeriod">Registration period in years.</param>
public sealed record DomainTermDto(
    DateTimeOffset RegisteredAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset NextDueDate,
    int RegistrationPeriod);
