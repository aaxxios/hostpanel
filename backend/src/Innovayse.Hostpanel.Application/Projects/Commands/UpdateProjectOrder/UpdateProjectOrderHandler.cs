using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Projects.Commands.UpdateProjectOrder;

/// <summary>
/// Applies the new sort order values for the ids listed in the command and persists the reordering in one
/// transaction. Projects whose ids are not in the command are left unchanged.
/// </summary>
public sealed class UpdateProjectOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateProjectOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The project id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success. Unknown ids in the list are silently skipped.</returns>
    public async Task<Result> HandleAsync(UpdateProjectOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var orderMap = cmd.Items.GroupBy(i => i.Id).ToDictionary(g => g.Key, g => g.Last().SortOrder);
        var ids = orderMap.Keys.ToList();
        var rows = await _context.Projects.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

        foreach (var row in rows)
        {
            row.SetSortOrder(orderMap[row.Id]);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
