using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetClientsReport;

/// <summary>Handles <see cref="GetClientsReportQuery"/>.</summary>
/// <remarks>
/// The sign-in address is not on the client row itself — it belongs to whichever system owns
/// accounts, so it is resolved for the one page returned through <see cref="IIdentityProvider"/>
/// rather than by joining the local Identity user table, which is empty in SSO mode.
/// </remarks>
public sealed class GetClientsReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves each row's sign-in email address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Resolves each row's sign-in email address.</param>
    public GetClientsReportHandler(IHostpanelDbContext context, IIdentityProvider identity)
    {
        _context = context;
        _identity = identity;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters and page to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching page of clients and the unfiltered total. Never refuses.</returns>
    public async Task<Result<ClientReportResultDto>> HandleAsync(GetClientsReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clients = _context.Clients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            clients = clients.Where(c => c.Status.ToString() == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Country))
        {
            clients = clients.Where(c => c.Country != null && c.Country.ToLower().Contains(query.Country.ToLower()));
        }

        if (query.CreatedFrom.HasValue)
        {
            var from = query.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            clients = clients.Where(c => c.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var to = query.CreatedTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            clients = clients.Where(c => c.CreatedAt <= to);
        }

        var totalCount = await clients.CountAsync(ct);

        var page = await clients
            .OrderByDescending(c => c.CreatedAt)
            .Paginate(query.Page, query.PageSize)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.CompanyName,
                c.UserId,
                c.Street,
                c.Address2,
                c.City,
                c.State,
                c.PostCode,
                c.Country,
                c.Phone,
                c.Currency,
                c.CreditBalance,
                c.Status,
                c.CreatedAt,
                c.AdminNotes,
            })
            .ToListAsync(ct);

        var emails = await _identity.GetEmailsBySubjectsAsync(page.Select(c => c.UserId), ct);

        IReadOnlyList<ClientReportDto> items = page.Select(c => new ClientReportDto(
            c.Id,
            c.FirstName,
            c.LastName,
            c.CompanyName,
            emails.GetValueOrDefault(c.UserId, string.Empty),
            c.Street,
            c.Address2,
            c.City,
            c.State,
            c.PostCode,
            c.Country,
            c.Phone,
            c.Currency,
            c.CreditBalance,
            c.Status.ToString(),
            c.CreatedAt.ToString("yyyy-MM-dd"),
            c.AdminNotes)).ToList();

        return Result<ClientReportResultDto>.Success(new ClientReportResultDto(items, totalCount));
    }
}
