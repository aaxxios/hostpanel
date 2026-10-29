namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>A signed-in client account on a host whose mail relay is a fake, for the address-change tests.</summary>
/// <param name="Client">The client carrying the account's bearer token.</param>
/// <param name="Host">The host the account lives on; a fresh client of it is unauthenticated.</param>
/// <param name="Email">The address the account signs in with.</param>
/// <param name="Sender">The fake relay every mail of the host is recorded by.</param>
public sealed record EmailChangeAccount(
    HttpClient Client,
    Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> Host,
    string Email,
    FakeEmailSender Sender);
