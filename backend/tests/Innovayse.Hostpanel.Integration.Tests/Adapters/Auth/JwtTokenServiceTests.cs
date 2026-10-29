using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Auth;

/// <summary>
/// The access token carries exactly what <see cref="AccessTokenClaims"/> says about the person.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class JwtTokenServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Subject, address, verification flag, names, roles and the session version all reach the token.</summary>
    [Fact]
    public void GenerateAccessToken_CarriesEveryClaimItWasGiven()
    {
        var jwt = factory.Services.GetRequiredService<IJwtService>();

        var token = jwt.GenerateAccessToken(new AccessTokenClaims(
            "user-1", "ada@example.com", "Ada", "Lovelace", [Roles.Admin], EmailVerified: true, SessionVersion: "version-1"));

        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();
        claims.Should().Contain(c => c.Type == "sub" && c.Value == "user-1");
        claims.Should().Contain(c => c.Type == "email" && c.Value == "ada@example.com");
        claims.Should().Contain(c => c.Type == "email_verified" && c.Value == "true");
        claims.Should().Contain(c => c.Type == "given_name" && c.Value == "Ada");
        claims.Should().Contain(c => c.Type == "family_name" && c.Value == "Lovelace");
        claims.Should().Contain(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == Roles.Admin);
        claims.Should().Contain(c => c.Type == "session_version" && c.Value == "version-1");
    }

    /// <summary>Unknown names are left off rather than carried empty, and an unconfirmed address says so.</summary>
    [Fact]
    public void GenerateAccessToken_OmitsUnknownNamesAndReportsAnUnverifiedAddress()
    {
        var jwt = factory.Services.GetRequiredService<IJwtService>();

        var token = jwt.GenerateAccessToken(new AccessTokenClaims(
            "user-2", "grace@example.com", FirstName: null, LastName: null, [], EmailVerified: false, SessionVersion: "version-2"));

        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToList();
        claims.Should().Contain(c => c.Type == "email_verified" && c.Value == "false");
        claims.Should().NotContain(c => c.Type == "given_name" || c.Type == "family_name");
    }
}
