namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An identity account registered through the public registration endpoint.
/// </summary>
/// <param name="UserId">The subject id of the account.</param>
/// <param name="Email">The email address the account signs in with.</param>
public sealed record RegisteredAccount(string UserId, string Email);
