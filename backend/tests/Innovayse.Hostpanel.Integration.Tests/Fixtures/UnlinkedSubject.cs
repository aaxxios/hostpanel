namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An identity subject created for a test with no client row linked to it.
/// </summary>
/// <param name="UserId">The subject id of the account.</param>
/// <param name="Email">The email address the account signs in with.</param>
public sealed record UnlinkedSubject(string UserId, string Email);
