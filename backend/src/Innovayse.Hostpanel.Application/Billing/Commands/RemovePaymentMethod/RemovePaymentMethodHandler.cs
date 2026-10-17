using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.RemovePaymentMethod;

/// <summary>
/// Handles <see cref="RemovePaymentMethodCommand"/>.
/// </summary>
public sealed class RemovePaymentMethodHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Card payment gateway, for detaching the payment method.</summary>
    private readonly ICardPaymentGateway _cardGateway;

    /// <summary>Who is asking; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cardGateway">Card payment gateway, for detaching the payment method.</param>
    /// <param name="caller">Who is asking; the command does not say, and must not.</param>
    public RemovePaymentMethodHandler(IHostpanelDbContext context, ICardPaymentGateway cardGateway, ICurrentUserService caller)
    {
        _context = context;
        _cardGateway = cardGateway;
        _caller = caller;
    }

    /// <summary>
    /// Detaches the given payment method from the client's card gateway customer.
    /// </summary>
    /// <param name="cmd">The command. It names no account: this acts on the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record,
    /// <c>NO_SAVED_PAYMENT_METHODS</c> when the client has never saved one, or
    /// <c>PAYMENT_METHOD_NOT_OWNED</c> when the id does not belong to this client.
    /// </returns>
    public async Task<Result> HandleAsync(RemovePaymentMethodCommand cmd, CancellationToken ct)
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

        // Detach takes a bare payment method ID with no customer scoping, so without this
        // check a client could remove a payment method that belongs to somebody else's
        // card gateway customer just by knowing (or guessing) its ID.
        var methods = await _cardGateway.ListPaymentMethodsAsync(client.CardGatewayCustomerId, ct);
        if (!methods.Any(m => m.Id == cmd.PaymentMethodId))
        {
            return Result.FailureCode(HostpanelErrorCodes.PAYMENT_METHOD_NOT_OWNED);
        }

        await _cardGateway.DetachPaymentMethodAsync(cmd.PaymentMethodId, ct);

        return Result.Success();
    }
}
