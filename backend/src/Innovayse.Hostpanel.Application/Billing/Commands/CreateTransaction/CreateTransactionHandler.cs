using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateTransaction;

/// <summary>
/// Creates a <see cref="Transaction"/> and optionally adjusts the client's credit balance.
/// </summary>
public sealed class CreateTransactionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Current request context, providing the acting admin's identity and IP for the audit trail.</summary>
    private readonly ICurrentUserService _ctx;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="ctx">Current request context providing admin identity and IP.</param>
    public CreateTransactionHandler(IHostpanelDbContext context, ICurrentUserService ctx)
    {
        _context = context;
        _ctx = ctx;
    }

    /// <summary>Handles <see cref="CreateTransactionCommand"/>.</summary>
    /// <param name="cmd">The create command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the newly created transaction's ID, or <c>CLIENT_NOT_FOUND</c> when
    /// <paramref name="cmd"/> asks for a credit adjustment against a client that does not exist.
    /// </returns>
    public async Task<Result<int>> HandleAsync(CreateTransactionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var transaction = Transaction.Create(
            cmd.ClientId,
            cmd.Date,
            cmd.Description,
            cmd.TransactionId,
            cmd.InvoiceId,
            cmd.PaymentMethod,
            cmd.AmountIn,
            cmd.AmountOut,
            cmd.Fees,
            cmd.AddToCredit);

        _context.Transactions.Add(transaction);

        if (cmd.AddToCredit)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
            if (client is null)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
            }

            if (cmd.AmountIn > 0)
            {
                client.AddCredit(cmd.AmountIn);
            }

            if (cmd.AmountOut > 0)
            {
                client.DeductCredit(cmd.AmountOut);
            }
        }

        await _context.SaveChangesAsync(ct);

        _context.ActivityLogs.Add(ActivityLog.Create(
            cmd.ClientId,
            $"Added Transaction - Transaction ID: {transaction.Id}",
            _ctx.UserId, _ctx.UserName, _ctx.Email, _ctx.IpAddress));
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(transaction.Id);
    }
}
