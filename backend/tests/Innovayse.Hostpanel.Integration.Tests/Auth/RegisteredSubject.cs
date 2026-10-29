namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// An account registered for a logout test.
/// </summary>
/// <param name="Email">The account's email.</param>
/// <param name="Subject">The account's subject id.</param>
public sealed record RegisteredSubject(string Email, string Subject);
