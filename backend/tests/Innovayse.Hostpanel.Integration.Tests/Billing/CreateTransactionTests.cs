using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/transactions</c> against Postgres: recording a manual transaction on a client's
/// ledger, and the round-trip through a subsequent read.
/// </summary>
/// <remarks>Split from the deleted <c>TransactionsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateTransactionTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating a transaction, then reading it back, round-trips its amounts.</summary>
    [Fact]
    public async Task CreateThenGetRoundTripsTheTransaction()
    {
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var created = await admin.PostAsJsonAsync("/api/transactions", new
        {
            clientId,
            date = DateTimeOffset.UtcNow,
            description = "Manual top-up",
            transactionId = $"txn-{Guid.NewGuid():N}",
            invoiceId = (int?)null,
            paymentMethod = PaymentMethodNames.Manual,
            amountIn = 25.00m,
            amountOut = 0m,
            fees = 0m,
            addToCredit = false,
        });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var transactionId = await created.Content.ReadFromJsonAsync<int>();

        var fetched = await admin.GetAsync($"/api/transactions/{transactionId}");
        fetched.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("id").GetInt32().Should().Be(transactionId);
        body.RootElement.GetProperty("amountIn").GetDecimal().Should().Be(25.00m);
    }

    /// <summary>Creating a transaction is not for anonymous callers.</summary>
    [Fact]
    public async Task CreateRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/transactions", new
        {
            clientId = 1,
            date = DateTimeOffset.UtcNow,
            description = "Manual top-up",
            transactionId = "txn-x",
            invoiceId = (int?)null,
            paymentMethod = PaymentMethodNames.Manual,
            amountIn = 25.00m,
            amountOut = 0m,
            fees = 0m,
            addToCredit = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
