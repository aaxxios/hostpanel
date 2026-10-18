using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SetDefaultPaymentMethod;

/// <summary>
/// Handles <see cref="SetDefaultPaymentMethodCommand"/>.
/// </summary>
public sealed class SetDefaultPaymentMethodHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Card payment gateway, for updating the customer's default payment method.</summary>
    private readonly ICardPaymentGateway _cardGateway;

    /// <summary>Who is asking; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cardGateway">Card payment gateway, for updating the customer's default payment method.</param>
    /// <param name="caller">Who is asking; the command does not say, and must not.</param>
    public SetDefaultPaymentMethodHandler(
        IHostpanelDbContext context, ICardPaymentGateway cardGateway, ICurrentUserService caller)
    {
        _context = context;
        _cardGateway = cardGateway;
        _caller = caller;
    }

    /// <summary>
    /// Makes the given payment method the client's default.
    /// </summary>
    /// <param name="cmd">The command. It names no account: this acts on the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record,
    /// <c>NO_SAVED_PAYMENT_METHODS</c> when the client has never saved one, or
    /// <c>PAYMENT_METHOD_NOT_OWNED</c> when the id does not belong to this client.
    /// </returns>
    public async Task<Result> HandleAsync(SetDefaultPaymentMethodCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var userId = _caller.RequireUserId();

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        if (client.CardGatewayCustomerId is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.NO_SAVED_PAYMENT_METHODS);
        }

        // The card gateway's own API would reject a payment method that isn't attached to this customer,
        // but checking here first means a mismatched ID gets this handler's own error rather
        // than an opaque gateway API exception -- and it keeps the same check in place if that
        // gateway-side validation ever turns out to be less strict than assumed.
        var methods = await _cardGateway.ListPaymentMethodsAsync(client.CardGatewayCustomerId, ct);
        if (!methods.Any(m => m.Id == cmd.PaymentMethodId))
        {
            return Result.FailureCode(HostpanelErrorCodes.PAYMENT_METHOD_NOT_OWNED);
        }

        await _cardGateway.SetDefaultPaymentMethodAsync(client.CardGatewayCustomerId, cmd.PaymentMethodId!, ct);

        return Result.Success();
    }
}
