namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// A domain owner, their domain and a domain owned by someone else.
/// </summary>
/// <param name="Owner">Signed in as the owner.</param>
/// <param name="ClientId">The owner's client id.</param>
/// <param name="DomainId">The owner's domain id.</param>
/// <param name="OtherDomainId">A domain the owner does not own.</param>
public sealed record RenewableDomainScenario(HttpClient Owner, int ClientId, int DomainId, int OtherDomainId);
