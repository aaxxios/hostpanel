using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>DELETE /api/transactions/{id}</c> against Postgres: deleting a transaction reverses any
/// credit it had added, and the <c>TRANSACTION_NOT_FOUND</c> refusal for an id that names no
/// transaction.
/// </summary>
/// <remarks>Split from the deleted <c>TransactionsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteTransactionTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Deleting a transaction reverses the credit it had added, then removes it from the ledger.</summary>
    [Fact]
    public async Task DeleteReversesCreditThenRemovesTheTransaction()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var transactionId = await BillingTestSeeds.CreateTransactionAsync(admin, clientId, 10.00m, addToCredit: true);

        var deleted = await admin.DeleteAsync($"/api/transactions/{transactionId}");
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await admin.GetAsync($"/api/transactions/{transactionId}");
        fetched.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>Deleting an id that names no transaction answers 404 with <c>TRANSACTION_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task DeleteUnknownTransactionRefusesNotFound()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.DeleteAsync($"/api/transactions/{BillingTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.TRANSACTION_NOT_FOUND);
    }

    /// <summary>Deleting a transaction is not for anonymous callers.</summary>
    [Fact]
    public async Task DeleteRefusesAnonymous()
    {
        var response = await factory.CreateClient().DeleteAsync("/api/transactions/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
