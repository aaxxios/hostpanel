namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The authorization code a gaining registrar asks for when a domain moves away.</summary>
/// <param name="EppCode">The EPP authorization code the registrar issued for the transfer.</param>
public record EppCodeDto(string EppCode);
