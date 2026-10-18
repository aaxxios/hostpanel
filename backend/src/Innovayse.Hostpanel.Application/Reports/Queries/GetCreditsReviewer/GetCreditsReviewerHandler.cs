using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetCreditsReviewer;

/// <summary>Handles <see cref="GetCreditsReviewerQuery"/>.</summary>
public sealed class GetCreditsReviewerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetCreditsReviewerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every matching credit, newest first, with the count and sum of what matched. Never refuses.</returns>
    public async Task<Result<CreditsReviewerDto>> HandleAsync(GetCreditsReviewerQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var transactions = _context.Transactions
            .AsNoTracking()
            .Where(t => t.AddedToCredit && t.AmountIn > 0);

        if (query.ClientId.HasValue)
        {
            transactions = transactions.Where(t => t.ClientId == query.ClientId.Value);
        }

        if (query.From.HasValue)
        {
            var fromDt = new DateTimeOffset(query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            transactions = transactions.Where(t => t.Date >= fromDt);
        }

        if (query.To.HasValue)
        {
            var toDt = new DateTimeOffset(query.To.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
            transactions = transactions.Where(t => t.Date <= toDt);
        }

        if (query.MinAmount.HasValue)
        {
            transactions = transactions.Where(t => t.AmountIn >= query.MinAmount.Value);
        }

        if (query.MaxAmount.HasValue)
        {
            transactions = transactions.Where(t => t.AmountIn <= query.MaxAmount.Value);
        }

        var raw = await transactions
            .OrderByDescending(t => t.Date)
            .Join(_context.Clients.AsNoTracking(), t => t.ClientId, c => c.Id, (t, c) => new
            {
                t.Id,
                ClientId = c.Id,
                ClientName = c.FirstName + " " + c.LastName,
                t.Date,
                t.Description,
                Amount = t.AmountIn,
            })
            .ToListAsync(ct);

        var rows = raw.Select(r => new CreditsReviewerRowDto(
            r.Id, r.ClientId, r.ClientName,
            r.Date.ToString("dd/MM/yyyy"),
            r.Description, r.Amount, AdminUser: null)).ToList();

        return Result<CreditsReviewerDto>.Success(new CreditsReviewerDto(rows.Count, rows.Sum(r => r.Amount), rows));
    }
}
