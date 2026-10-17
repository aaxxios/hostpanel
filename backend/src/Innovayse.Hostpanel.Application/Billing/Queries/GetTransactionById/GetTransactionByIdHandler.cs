using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetTransactionById;

/// <summary>Handles <see cref="GetTransactionByIdQuery"/>.</summary>
public sealed class GetTransactionByIdHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetTransactionByIdHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Returns the transaction with the given ID.</summary>
    /// <param name="query">The query carrying the transaction ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the transaction, or <c>TRANSACTION_NOT_FOUND</c> when the id names no transaction.
    /// </returns>
    public async Task<Result<TransactionDto>> HandleAsync(GetTransactionByIdQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var t = await _context.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (t is null)
        {
            return Result<TransactionDto>.FailureCode(HostpanelErrorCodes.TRANSACTION_NOT_FOUND);
        }

        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == t.ClientId, ct);
        var clientName = client is not null
            ? $"{client.FirstName} {client.LastName}".Trim()
            : $"Client #{t.ClientId}";

        return Result<TransactionDto>.Success(new TransactionDto(
            t.Id, t.ClientId, clientName, t.Date, t.Description, t.TransactionId,
            t.InvoiceId, t.PaymentMethod, t.AmountIn, t.AmountOut, t.Fees, t.AddedToCredit));
    }
}
