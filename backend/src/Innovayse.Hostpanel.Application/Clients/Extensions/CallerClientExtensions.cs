using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyCardGatewayCustomer;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>Reaches the card gateway customer of the account the caller acts for.</summary>
public static class CallerClientExtensions
{
    /// <summary>Resolves the card gateway customer of the account the caller acts for, owner or linked user.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the query through.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The link, or the <c>CLIENT_PROFILE_NOT_FOUND</c> refusal.</returns>
    public static Task<Result<CardGatewayCustomerDto>> GetCallerCardGatewayCustomerAsync(this IMessageBus bus, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        return bus.InvokeAsync<Result<CardGatewayCustomerDto>>(new GetMyCardGatewayCustomerQuery(), ct);
    }
}
