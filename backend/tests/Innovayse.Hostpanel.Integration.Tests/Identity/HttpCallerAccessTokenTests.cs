using System.IdentityModel.Tokens.Jwt;
using Innovayse.Hostpanel.API.Identity;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Integration.Tests.Identity;

/// <summary>
/// The adapter that hands the caller's own bearer token to outbound calls made on the caller's behalf.
/// </summary>
/// <remarks>
/// Anything that is not a usable bearer token issued by the external provider must read as null, so
/// the caller of the port refuses instead of forwarding an empty, foreign or locally minted credential.
/// </remarks>
public sealed class HttpCallerAccessTokenTests
{
    /// <summary>The issuer this product signs its own tokens with in these tests.</summary>
    private const string LocalIssuer = "hostpanel-local";

    /// <summary>A token the external provider issued.</summary>
    private static readonly string Token = TokenFrom("https://accounts.example.com");

    /// <summary>Builds the adapter over a request carrying the given <c>Authorization</c> header.</summary>
    /// <param name="authorization">The header value; null means the request carries none.</param>
    /// <returns>The adapter under test.</returns>
    private static HttpCallerAccessToken Build(string? authorization)
    {
        var context = new DefaultHttpContext();
        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        return new HttpCallerAccessToken(new HttpContextAccessor { HttpContext = context }, LocalOptions());
    }

    /// <summary>The local token settings, naming <see cref="LocalIssuer"/>.</summary>
    /// <returns>The options the adapter is built with.</returns>
    private static IOptions<JwtOptions> LocalOptions()
    {
        return Options.Create(new JwtOptions { Issuer = LocalIssuer });
    }

    /// <summary>Writes an unsigned token naming the given issuer; the adapter reads only the issuer.</summary>
    /// <param name="issuer">The issuer the token names.</param>
    /// <returns>The serialized token.</returns>
    private static string TokenFrom(string issuer)
    {
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer: issuer, claims: []));
    }

    /// <summary>A token this product minted itself is never forwarded to the provider.</summary>
    [Fact]
    public void GetBearerToken_WithALocallyIssuedToken_IsNull()
    {
        Build($"Bearer {TokenFrom(LocalIssuer)}").GetBearerToken().Should().BeNull();
    }

    /// <summary>A bearer value that is not a readable token is never forwarded.</summary>
    [Fact]
    public void GetBearerToken_WithAnUnreadableToken_IsNull()
    {
        Build("Bearer not-a-token").GetBearerToken().Should().BeNull();
    }

    /// <summary>A bearer header yields the token after the scheme.</summary>
    [Fact]
    public void GetBearerToken_WithABearerHeader_ReturnsTheToken()
    {
        Build($"Bearer {Token}").GetBearerToken().Should().Be(Token);
    }

    /// <summary>The scheme name is matched without regard to case, as HTTP defines it.</summary>
    [Fact]
    public void GetBearerToken_WithALowerCaseScheme_ReturnsTheToken()
    {
        Build($"bearer {Token}").GetBearerToken().Should().Be(Token);
    }

    /// <summary>A request with no header has no token.</summary>
    [Fact]
    public void GetBearerToken_WithNoHeader_IsNull()
    {
        Build(null).GetBearerToken().Should().BeNull();
    }

    /// <summary>A header under another scheme is not a bearer token and is never forwarded.</summary>
    [Fact]
    public void GetBearerToken_WithAnotherScheme_IsNull()
    {
        Build("Basic dXNlcjpwYXNz").GetBearerToken().Should().BeNull();
    }

    /// <summary>The scheme with nothing after it carries no token.</summary>
    [Fact]
    public void GetBearerToken_WithAnEmptyToken_IsNull()
    {
        Build("Bearer ").GetBearerToken().Should().BeNull();
    }

    /// <summary>Outside a request, such as in a background job, there is no caller and no token.</summary>
    [Fact]
    public void GetBearerToken_WithNoHttpContext_IsNull()
    {
        new HttpCallerAccessToken(new HttpContextAccessor(), LocalOptions()).GetBearerToken().Should().BeNull();
    }
}
