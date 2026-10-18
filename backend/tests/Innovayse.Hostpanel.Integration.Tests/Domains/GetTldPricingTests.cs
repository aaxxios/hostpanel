using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the anonymous <c>GetTldPricing</c> use case —
/// <c>GET /api/domains/tld-pricing</c>, which the storefront and the client area both need
/// before or without a session. Had no Integration coverage of its own before this batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetTldPricingTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An anonymous caller reaches the endpoint and gets 200 with a currency block, even with nothing configured.</summary>
    [Fact]
    public async Task GetTldPricing_AnonymousCaller_Returns200WithDefaultCurrencyAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/domains/tld-pricing");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("currency").GetProperty("code").GetString().Should().Be("USD");
    }

    /// <summary>An enabled TLD configuration is included in the pricing table, converted to the requested currency.</summary>
    [Fact]
    public async Task GetTldPricing_IncludesAnEnabledTldConfigAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var tld = TldConfig.Create(
            $"pricing-{Guid.NewGuid():N}"[..12], RegistrarModule.NameAm, "USD", "USD",
            isEnabled: true, sortOrder: 0, categories: []);
        tld.UpdateCostPrices(
            register: new Dictionary<int, decimal> { [1] = 5m },
            transfer: new Dictionary<int, decimal> { [1] = 5m },
            renew: new Dictionary<int, decimal> { [1] = 5m });
        tld.UpdateSellPrices(
            register: new Dictionary<int, decimal> { [1] = 10m },
            transfer: new Dictionary<int, decimal> { [1] = 10m },
            renew: new Dictionary<int, decimal> { [1] = 10m });
        context.TldConfigs.Add(tld);
        await context.SaveChangesAsync(CancellationToken.None);

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/domains/tld-pricing?currency=USD");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain(tld.Tld);
    }

    /// <summary>
    /// AMD prices follow the stored exchange rate: a 10 USD sell price is 10 / rate AMD, and
    /// changing the rate changes the price on the next call (no hard-coded table).
    /// </summary>
    [Fact]
    public async Task GetTldPricing_AmdPricesFollowTheStoredExchangeRateAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var amd = await context.Currencies.SingleAsync(c => c.Code == "AMD");
        var originalRate = amd.RateToBase;
        var tld = TldConfig.Create(
            $"rate-{Guid.NewGuid():N}"[..12], RegistrarModule.NameAm, "USD", "USD",
            isEnabled: true, sortOrder: 0, categories: []);
        tld.UpdateSellPrices(
            register: new Dictionary<int, decimal> { [1] = 10m },
            transfer: new Dictionary<int, decimal> { [1] = 10m },
            renew: new Dictionary<int, decimal> { [1] = 10m });
        context.TldConfigs.Add(tld);

        try
        {
            var client = factory.CreateClient();
            foreach (var rate in new[] { 0.0025m, 0.005m })
            {
                amd.UpdateRate(rate);
                await context.SaveChangesAsync(CancellationToken.None);
                cache.Remove("tld-pricing-configs");

                var body = await client.GetFromJsonAsync<JsonElement>("/api/domains/tld-pricing?currency=AMD");

                var expected = amd.Round(10m / rate).ToString($"F{amd.Decimals}", System.Globalization.CultureInfo.InvariantCulture);
                body.GetProperty("pricing").GetProperty(tld.Tld).GetProperty("register").GetProperty("1")
                    .GetString().Should().Be(expected);
            }
        }
        finally
        {
            amd.UpdateRate(originalRate);
            context.TldConfigs.Remove(tld);
            await context.SaveChangesAsync(CancellationToken.None);
            cache.Remove("tld-pricing-configs");
        }
    }
}
