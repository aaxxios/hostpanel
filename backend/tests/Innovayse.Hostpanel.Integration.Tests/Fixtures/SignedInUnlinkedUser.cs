namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A signed-in user who owns no client account and is linked to none.
/// </summary>
/// <param name="Client">The HTTP client carrying the user's bearer token.</param>
/// <param name="UserId">The user's identity subject.</param>
/// <param name="Email">The user's email address.</param>
public sealed record SignedInUnlinkedUser(HttpClient Client, string UserId, string Email);
