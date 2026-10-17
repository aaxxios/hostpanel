using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Innovayse.Hostpanel.API.Auth;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// Sliding renewal of the local session: a token in the last third of its life is re-issued, an
/// expired one is refused, and no renewal ever outlives the absolute maximum session age.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class LocalSessionRenewalTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A route any signed-in caller may read.</summary>
    private const string Route = "/api/auth/email-verified";

    /// <summary>Access-token length the test hosts run with, in minutes.</summary>
    private const int AccessMinutes = 15;

    /// <summary>A token in the last third of its life is renewed into a later expiry.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task TokenNearExpiry_IsRenewedInTheCookie()
    {
        var (client, clock, jwt) = Host(maxSessionMinutes: 720);
        var token = Mint(jwt);
        clock.Advance(TimeSpan.FromMinutes(12));

        var response = await GetWithCookieAsync(client, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var renewed = RenewedCookie(response);
        renewed.Should().NotBeNull();
        new JwtSecurityTokenHandler().ReadJwtToken(renewed).ValidTo
            .Should().BeAfter(new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo);
    }

    /// <summary>A token with most of its life left is left alone.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task TokenEarlyInItsLife_IsNotRenewed()
    {
        var (client, clock, jwt) = Host(maxSessionMinutes: 720);
        var token = Mint(jwt);
        clock.Advance(TimeSpan.FromMinutes(2));

        var response = await GetWithCookieAsync(client, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RenewedCookie(response).Should().BeNull();
    }

    /// <summary>An expired token is refused and nothing is issued in its place.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task ExpiredToken_IsRefusedAndNotRenewed()
    {
        var (client, clock, jwt) = Host(maxSessionMinutes: 720);
        clock.Advance(TimeSpan.FromMinutes(-30));
        var token = Mint(jwt);
        clock.Advance(TimeSpan.FromMinutes(30));

        var response = await GetWithCookieAsync(client, token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        RenewedCookie(response).Should().BeNull();
    }

    /// <summary>A renewal is capped at the absolute maximum session age.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Renewal_NeverOutlivesTheMaximumSessionAge()
    {
        var (client, clock, jwt) = Host(maxSessionMinutes: AccessMinutes);
        var start = clock.GetUtcNow();
        var token = Mint(jwt);
        clock.Advance(TimeSpan.FromMinutes(12));

        var response = await GetWithCookieAsync(client, token);

        var renewed = RenewedCookie(response);
        renewed.Should().NotBeNull();
        new DateTimeOffset(new JwtSecurityTokenHandler().ReadJwtToken(renewed).ValidTo, TimeSpan.Zero)
            .Should().BeOnOrBefore(start.AddMinutes(AccessMinutes));
    }

    /// <summary>A session past its maximum age is not renewed again.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task SessionPastTheMaximumAge_IsNotRenewed()
    {
        var (client, clock, jwt) = Host(maxSessionMinutes: AccessMinutes);
        var token = Mint(jwt);
        clock.Advance(TimeSpan.FromMinutes(AccessMinutes + 1));

        var response = await GetWithCookieAsync(client, token);

        RenewedCookie(response).Should().BeNull();
    }

    /// <summary>A bearer caller (the client portal server) gets the renewed token in a header.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task BearerCaller_GetsRenewedTokenInTheHeader()
    {
        var (client, clock, jwt) = Host(maxSessionMinutes: 720);
        var token = Mint(jwt);
        clock.Advance(TimeSpan.FromMinutes(12));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(Route);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues(LocalSessionRenewalMiddleware.RenewedTokenHeader).Should().ContainSingle();
    }

    /// <summary>Builds a host with a hand-moved clock and the given maximum session age.</summary>
    /// <param name="maxSessionMinutes">Absolute maximum session age, in minutes.</param>
    /// <returns>The client, the clock and the host token service.</returns>
    private (HttpClient Client, ManualTimeProvider Clock, IJwtService Jwt) Host(int maxSessionMinutes)
    {
        var clock = new ManualTimeProvider(DateTimeOffset.UtcNow);
        var host = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Session:AccessTokenMinutes", AccessMinutes.ToString());
            builder.UseSetting("Session:MaxSessionMinutes", maxSessionMinutes.ToString());
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(clock);
            });
        });

        return (host.CreateClient(), clock, host.Services.GetRequiredService<IJwtService>());
    }

    /// <summary>Mints an admin token at the clock current instant.</summary>
    /// <param name="jwt">The host token service.</param>
    /// <returns>The token.</returns>
    private static string Mint(IJwtService jwt)
    {
        return jwt.GenerateAccessToken("renewal-user", "renewal@example.com", null, null, [Roles.Admin]);
    }

    /// <summary>Calls the route presenting the token as the session cookie.</summary>
    /// <param name="client">The host client.</param>
    /// <param name="token">The token to present.</param>
    /// <returns>The response.</returns>
    private static Task<HttpResponseMessage> GetWithCookieAsync(HttpClient client, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, Route);
        request.Headers.Add("Cookie", $"{LocalSessionCookie.Name}={token}");
        return client.SendAsync(request);
    }

    /// <summary>Reads the session cookie value the response set, if any.</summary>
    /// <param name="response">The response.</param>
    /// <returns>The cookie token, or <see langword="null"/> when none was set.</returns>
    private static string? RenewedCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return null;
        }

        var prefix = $"{LocalSessionCookie.Name}=";
        var line = cookies.FirstOrDefault(c => c.StartsWith(prefix, StringComparison.Ordinal));
        return line?[prefix.Length..].Split(';')[0];
    }
}
