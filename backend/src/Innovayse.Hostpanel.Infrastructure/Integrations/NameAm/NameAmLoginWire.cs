namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>The body of Name.am's <c>POST /auth/login</c>.</summary>
/// <param name="Email">The reseller account e-mail.</param>
/// <param name="Password">The reseller account password.</param>
/// <param name="Token">The second-factor token field the API expects; always empty, as the account has none.</param>
internal sealed record NameAmLoginWire(string Email, string Password, string Token);
