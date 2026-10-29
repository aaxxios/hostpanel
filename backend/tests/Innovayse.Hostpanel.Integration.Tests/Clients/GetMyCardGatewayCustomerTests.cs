using System.Security.Claims;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyCardGatewayCustomer;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary>
/// <see cref="GetMyCardGatewayCustomerQuery"/> through the real <see cref="IMessageBus"/>: the query
/// the Billing module dispatches to reach the caller's saved payment methods.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMyCardGatewayCustomerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A client who never saved a payment method gets their record with no customer id.</summary>
    [Fact]
    public async Task HandleAsync_NoCustomerYet_ReturnsNullCustomerIdAsync()
    {
        var seeded = await ClientsTestSeeds.SeedClientAsync(factory);

        var result = await InvokeAsAsync(seeded.UserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new CardGatewayCustomerDto(seeded.ClientId, null));
    }

    /// <summary>A client linked to a card gateway customer gets that customer id.</summary>
    [Fact]
    public async Task HandleAsync_LinkedCustomer_ReturnsItAsync()
    {
        var seeded = await ClientsTestSeeds.SeedClientAsync(factory);
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var client = await context.Clients.FirstAsync(c => c.Id == seeded.ClientId, CancellationToken.None);
            client.SetCardGatewayCustomerId("cus_mine");
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var result = await InvokeAsAsync(seeded.UserId);

        result.Value.Should().Be(new CardGatewayCustomerDto(seeded.ClientId, "cus_mine"));
    }

    /// <summary>A caller who owns no client record is refused.</summary>
    [Fact]
    public async Task HandleAsync_NoClientRecord_ReturnsClientProfileNotFoundAsync()
    {
        var result = await InvokeAsAsync(ClientsTestSeeds.NoSuchUserId);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
    }

    /// <summary>Signs the given subject in and dispatches the query on a fresh scope's bus.</summary>
    /// <param name="userId">The identity subject to act as.</param>
    /// <returns>The handler's result.</returns>
    private async Task<Result<CardGatewayCustomerDto>> InvokeAsAsync(string userId)
    {
        using var scope = factory.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], authenticationType: "Test"));
        accessor.HttpContext = new DefaultHttpContext { User = principal };
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        return await bus.InvokeAsync<Result<CardGatewayCustomerDto>>(new GetMyCardGatewayCustomerQuery());
    }
}
