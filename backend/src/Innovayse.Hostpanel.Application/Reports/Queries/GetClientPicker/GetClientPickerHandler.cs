using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetClientPicker;

/// <summary>Handles <see cref="GetClientPickerQuery"/>.</summary>
public sealed class GetClientPickerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetClientPickerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query. Carries no filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every client's id and full name, ordered by first then last name. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<ClientPickerDto>>> HandleAsync(GetClientPickerQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Clients
            .AsNoTracking()
            .OrderBy(c => c.FirstName).ThenBy(c => c.LastName)
            .Select(c => new ClientPickerDto(c.Id, c.FirstName + " " + c.LastName))
            .ToListAsync(ct);

        return Result<IReadOnlyList<ClientPickerDto>>.Success(rows);
    }
}
