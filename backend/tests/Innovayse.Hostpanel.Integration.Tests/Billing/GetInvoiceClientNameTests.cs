using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/billing/{id}</c> when the owning client row is gone: the invoice is still returned
/// and its <c>clientName</c> is <c>null</c> rather than a made-up fallback string.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetInvoiceClientNameTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An invoice whose client no longer exists answers 200 with a null <c>clientName</c>.</summary>
    [Fact]
    public async Task GetInvoice_ClientMissing_ReturnsNullClientName()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 10m);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            await context.Clients.Where(c => c.Id == clientId).ExecuteDeleteAsync();
        }

        var response = await admin.GetAsync($"/api/billing/{invoiceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("id").GetInt32().Should().Be(invoiceId);
        body.RootElement.GetProperty("clientName").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
