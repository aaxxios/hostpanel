namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A signed-in account, the email address it registered with and its identity subject.
/// </summary>
/// <param name="Client">The HTTP client carrying the account's bearer token.</param>
/// <param name="Email">The email address the account registered with.</param>
/// <param name="UserId">The account's identity subject.</param>
public sealed record SignedInAccount(HttpClient Client, string Email, string UserId);
