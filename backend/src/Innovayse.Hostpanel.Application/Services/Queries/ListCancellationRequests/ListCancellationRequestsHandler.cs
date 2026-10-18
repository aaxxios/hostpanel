using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListCancellationRequests;

/// <summary>Handles listing cancellation requests with enriched service and client data.</summary>
public sealed class ListCancellationRequestsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListCancellationRequestsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the query. Enriches each request with the product name and client full name.
    /// </summary>
    /// <param name="qry">The page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, always: an out-of-range page answers an empty page, not a refusal.</returns>
    public async Task<Result<PagedResult<CancellationRequestDto>>> HandleAsync(
        ListCancellationRequestsQuery qry, CancellationToken ct)
    {
        // Was ICancellationRequestRepository.ListAsync -- read-only, so no tracking is needed.
        var total = await _context.CancellationRequests.CountAsync(ct);
        var requests = await _context.CancellationRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Paginate(qry.Page, qry.PageSize)
            .ToListAsync(ct);

        var dtos = new List<CancellationRequestDto>(requests.Count);

        foreach (var req in requests)
        {
            // Was IClientServiceRepository.FindByIdAsync -- read-only, so no tracking is needed.
            var service = await _context.ClientServices
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == req.ServiceId, ct);
            var productName = "Unknown";
            var clientId = 0;
            var clientName = "Unknown";

            if (service is not null)
            {
                clientId = service.ClientId;

                // Was IProductRepository.FindByIdAsync -- read-only, so no tracking is needed.
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == service.ProductId, ct);
                productName = product?.Name ?? "Unknown";

                // Was IClientRepository.FindByIdAsync -- read-only, so no tracking is needed.
                var client = await _context.Clients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == service.ClientId, ct);
                clientName = client is not null
                    ? $"{client.FirstName} {client.LastName}"
                    : "Unknown";
            }

            dtos.Add(new CancellationRequestDto(
                req.Id,
                req.ServiceId,
                productName,
                clientId,
                clientName,
                req.Type.ToString(),
                req.Reason,
                req.Status.ToString(),
                req.CreatedAt));
        }

        return Result<PagedResult<CancellationRequestDto>>.Success(
            new PagedResult<CancellationRequestDto>(dtos, total, qry.Page, qry.PageSize));
    }
}
