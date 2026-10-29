namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An admin, an owner with one registered domain, and a stranger who does not own it.
/// </summary>
/// <param name="Admin">The HTTP client signed in as an administrator.</param>
/// <param name="Owner">The HTTP client signed in as the domain's owner.</param>
/// <param name="DomainId">The id of the owner's domain.</param>
/// <param name="Stranger">The HTTP client signed in as a user who owns nothing.</param>
public sealed record OwnedDomainScenario(HttpClient Admin, HttpClient Owner, int DomainId, HttpClient Stranger);
