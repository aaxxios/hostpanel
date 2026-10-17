using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateTransaction;

/// <summary>Updates an existing <see cref="Domain.Billing.Transaction"/>.</summary>
public sealed class UpdateTransactionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Current request context, providing the acting admin's identity and IP for the audit trail.</summary>
    private readonly ICurrentUserService _ctx;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="ctx">Current request context providing admin identity and IP.</param>
    public UpdateTransactionHandler(IHostpanelDbContext context, ICurrentUserService ctx)
    {
        _context = context;
        _ctx = ctx;
    }

    /// <summary>Handles <see cref="UpdateTransactionCommand"/>.</summary>
    /// <param name="cmd">The update command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>TRANSACTION_NOT_FOUND</c> when the id names no transaction.</returns>
    public async Task<Result> HandleAsync(UpdateTransactionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);
        if (transaction is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TRANSACTION_NOT_FOUND);
        }

        transaction.Update(
            cmd.Date,
            cmd.Description,
            cmd.TransactionId,
            cmd.InvoiceId,
            cmd.PaymentMethod,
            cmd.AmountIn,
            cmd.AmountOut,
            cmd.Fees);

        await _context.SaveChangesAsync(ct);

        _context.ActivityLogs.Add(ActivityLog.Create(
            transaction.ClientId,
            $"Updated Transaction - Transaction ID: {transaction.Id}",
            _ctx.UserId, _ctx.UserName, _ctx.Email, _ctx.IpAddress));
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
