using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteTransaction;

/// <summary>
/// Deletes a <see cref="Domain.Billing.Transaction"/> and reverses any credit adjustments.
/// </summary>
public sealed class DeleteTransactionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteTransactionHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles <see cref="DeleteTransactionCommand"/>.</summary>
    /// <param name="cmd">The delete command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>TRANSACTION_NOT_FOUND</c> when the id names no transaction, or
    /// <c>CLIENT_NOT_FOUND</c> when the transaction's credit adjustment cannot be reversed because
    /// the owning client no longer exists.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteTransactionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);
        if (transaction is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TRANSACTION_NOT_FOUND);
        }

        if (transaction.AddedToCredit)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == transaction.ClientId, ct);
            if (client is null)
            {
                return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
            }

            if (transaction.AmountIn > 0)
            {
                client.DeductCredit(transaction.AmountIn);
            }

            if (transaction.AmountOut > 0)
            {
                client.AddCredit(transaction.AmountOut);
            }
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
