using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing.Currencies;

/// <summary>
/// "Update rates" (<c>POST /api/admin/currencies/update-rates</c>) against Postgres, with the
/// upstream rate service replaced by a fake: what the source quotes is written, what it does not
/// quote keeps the rate it had and is reported back.
/// </summary>
/// <remarks>Split from the deleted <c>Billing/UpdateExchangeRatesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateExchangeRatesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The rate the fake source quotes for AMD, different from the seeded one.</summary>
    private const decimal QuotedAmdRate = 0.0020000m;

    /// <summary>
    /// Builds an admin client whose host answers rate lookups from <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The fake rate source the host should use.</param>
    /// <returns>An authenticated client.</returns>
    private async Task<HttpClient> AdminClientAsync(FakeExchangeRateSource source)
    {
        var host = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IExchangeRateSource>(_ => source)));

        var token = await factory.GetAdminTokenAsync();
        var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>A quoted currency is rewritten; an unquoted one is listed as missing and keeps its rate.</summary>
    [Fact]
    public async Task UpdateRatesWritesWhatIsQuotedAndReportsTheRest()
    {
        var source = new FakeExchangeRateSource(new Dictionary<string, decimal> { ["AMD"] = QuotedAmdRate });
        var client = await AdminClientAsync(source);

        // A currency the fake does not quote, so the answer must report it rather than zero it.
        await client.PutAsJsonAsync("/api/admin/currencies/SEK", new
        {
            numeric = "752",
            prefix = string.Empty,
            suffix = " kr",
            decimals = 2,
            rateToBase = 0.1m,
            isEnabled = true,
        });

        var response = await client.PostAsync("/api/admin/currencies/update-rates", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("updated").EnumerateArray()
            .Select(e => e.GetString()).Should().Contain("AMD");
        body.RootElement.GetProperty("missing").EnumerateArray()
            .Select(e => e.GetString()).Should().Contain("SEK");

        var rows = await client.GetFromJsonAsync<List<JsonElement>>("/api/admin/currencies");
        rows.Should().NotBeNull();
        rows!.Single(r => r.GetProperty("code").GetString() == "AMD")
            .GetProperty("rateToBase").GetDecimal().Should().Be(QuotedAmdRate);
        rows!.Single(r => r.GetProperty("code").GetString() == "SEK")
            .GetProperty("rateToBase").GetDecimal().Should().Be(0.1m);
    }

    /// <summary>
    /// Only the non-base codes are asked for: the base's rate to itself is not a question, and asking
    /// for it is how a source ends up quoting 1.0 over the real figure.
    /// </summary>
    [Fact]
    public async Task UpdateRatesAsksOnlyForTheNonBaseCodes()
    {
        var source = new FakeExchangeRateSource(new Dictionary<string, decimal> { ["AMD"] = QuotedAmdRate });
        var client = await AdminClientAsync(source);

        await client.PostAsync("/api/admin/currencies/update-rates", content: null);

        source.RequestedBase.Should().NotBeNullOrEmpty();
        source.RequestedCodes.Should().NotContain(source.RequestedBase!);
        source.RequestedCodes.Should().Contain("AMD");
    }

    /// <summary>Updating rates is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateRatesRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync("/api/admin/currencies/update-rates", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
