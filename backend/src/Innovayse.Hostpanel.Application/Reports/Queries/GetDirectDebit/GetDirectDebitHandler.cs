using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDirectDebit;

/// <summary>Handles <see cref="GetDirectDebitQuery"/>.</summary>
public sealed class GetDirectDebitHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetDirectDebitHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query. Carries no filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every unpaid invoice on a direct-debit payment method, soonest due first. Never refuses.</returns>
    public async Task<Result<DirectDebitDto>> HandleAsync(GetDirectDebitQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var raw = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Unpaid &&
                        (i.PaymentMethod != null && i.PaymentMethod.ToLower().Contains(PaymentMethodNames.DirectDebitMarker)))
            .Join(_context.Clients.AsNoTracking(), i => i.ClientId, c => c.Id, (i, c) => new
            {
                InvoiceId = i.Id,
                ClientName = c.FirstName + " " + c.LastName,
                InvoiceDate = i.CreatedAt,
                i.DueDate,
                Subtotal = i.SubTotal,
                i.Tax,
                i.Credit,
                i.Total,
            })
            .OrderBy(r => r.DueDate)
            .ToListAsync(ct);

        var rows = raw.Select(r => new DirectDebitRowDto(
            r.InvoiceId,
            r.ClientName,
            r.InvoiceDate.ToString("dd/MM/yyyy"),
            r.DueDate.ToString("dd/MM/yyyy"),
            r.Subtotal, r.Tax, r.Credit, r.Total,
            null, null, null, null)).ToList();

        return Result<DirectDebitDto>.Success(new DirectDebitDto(rows));
    }
}
