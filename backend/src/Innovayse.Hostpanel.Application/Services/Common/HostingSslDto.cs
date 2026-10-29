namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>The TLS certificate a hosted domain presents, as the portal shows it.</summary>
/// <param name="Valid">Whether the certificate chains to a trusted authority and today lies inside its validity window.</param>
/// <param name="Issuer">Who issued it: the issuer's organisation, else its common name.</param>
/// <param name="StartDate">When its validity starts.</param>
/// <param name="ExpiryDate">When its validity ends.</param>
public sealed record HostingSslDto(bool Valid, string Issuer, DateTimeOffset StartDate, DateTimeOffset ExpiryDate);
