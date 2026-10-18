using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyPaymentMethods;

/// <summary>
/// Handles <see cref="GetMyPaymentMethodsQuery"/>.
/// </summary>
public sealed class GetMyPaymentMethodsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Card payment gateway, for reading saved payment methods.</summary>
    private readonly ICardPaymentGateway _cardGateway;

    /// <summary>Who is asking; the query does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cardGateway">Card payment gateway, for reading saved payment methods.</param>
    /// <param name="caller">Who is asking; the query does not say, and must not.</param>
    public GetMyPaymentMethodsHandler(IHostpanelDbContext context, ICardPaymentGateway cardGateway, ICurrentUserService caller)
    {
        _context = context;
        _cardGateway = cardGateway;
        _caller = caller;
    }

    /// <summary>
    /// Retrieves the saved payment methods for the authenticated client.
    /// </summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The client's saved cards and bank accounts, empty for a client who has never had one
    /// attached, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record.
    /// </returns>
    public async Task<Result<IReadOnlyList<CardPaymentMethodDto>>> HandleAsync(
        GetMyPaymentMethodsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var userId = _caller.RequireUserId();

        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is null)
        {
            return Result<IReadOnlyList<CardPaymentMethodDto>>.FailureCode(
                HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        IReadOnlyList<CardPaymentMethodDto> methods = client.CardGatewayCustomerId is null
            ? []
            : await _cardGateway.ListPaymentMethodsAsync(client.CardGatewayCustomerId, ct);

        return Result.Success(methods);
    }
}
