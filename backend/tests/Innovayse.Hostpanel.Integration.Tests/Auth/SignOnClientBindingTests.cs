using System.Security.Claims;
using Innovayse.Hostpanel.API.Auth;
using Innovayse.Hostpanel.Shared.Constants;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <see cref="SignOnClientBinding"/>: an external sign-on token is accepted only when it was issued to
/// this deployment's own client.
/// </summary>
public sealed class SignOnClientBindingTests
{
    /// <summary>The client id the deployment is registered under in these tests.</summary>
    private const string OwnClient = "hostpanel";

    /// <summary>Builds a principal carrying one claim.</summary>
    /// <param name="type">The claim type.</param>
    /// <param name="value">The claim value.</param>
    /// <returns>The principal.</returns>
    private static ClaimsPrincipal With(string type, string value) =>
        new(new ClaimsIdentity([new Claim(JwtClaims.Sub, "subject-1"), new Claim(type, value)], "Test"));

    /// <summary>A token issued to this client is accepted, whichever claim names the client.</summary>
    /// <param name="claimType">The claim that carries the client.</param>
    [Theory]
    [InlineData(JwtClaims.ClientId)]
    [InlineData(JwtClaims.AuthorizedParty)]
    public void TokenIssuedToThisClientIsAccepted(string claimType)
    {
        SignOnClientBinding.IsIssuedTo(With(claimType, OwnClient), OwnClient).Should().BeTrue();
    }

    /// <summary>A token issued to another client of the same provider is refused.</summary>
    [Fact]
    public void TokenIssuedToAnotherClientIsRefused()
    {
        SignOnClientBinding.IsIssuedTo(With(JwtClaims.ClientId, "another-app"), OwnClient).Should().BeFalse();
    }

    /// <summary>A token that names no client at all is refused.</summary>
    [Fact]
    public void TokenWithoutAClientIsRefused()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtClaims.Sub, "subject-1")], "Test"));

        SignOnClientBinding.IsIssuedTo(principal, OwnClient).Should().BeFalse();
        SignOnClientBinding.IsIssuedTo(null, OwnClient).Should().BeFalse();
    }
}
