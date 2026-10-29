using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListOverdueSuspendedServices;

/// <summary>Handles <see cref="ListOverdueSuspendedServicesQuery"/> with one read.</summary>
public sealed class ListOverdueSuspendedServicesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListOverdueSuspendedServicesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists the services.</summary>
    /// <param name="query">Marker, no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ids, ordered. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<int>>> HandleAsync(ListOverdueSuspendedServicesQuery query, CancellationToken ct)
    {
        _ = query;

        var ids = await _context.ClientServices.AsNoTracking()
            .Where(s => s.Status == ServiceStatus.Suspended && s.SuspensionSource == SuspensionSource.Overdue)
            .OrderBy(s => s.Id)
            .Select(s => s.Id)
            .ToListAsync(ct);

        return Result<IReadOnlyList<int>>.Success(ids);
    }
}
