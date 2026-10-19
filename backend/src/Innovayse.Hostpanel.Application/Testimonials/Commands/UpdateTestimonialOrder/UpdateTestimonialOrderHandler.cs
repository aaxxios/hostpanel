using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonialOrder;

/// <summary>
/// Applies the new sort order values for the ids listed in the command and persists the reordering in one
/// transaction. Testimonials whose ids are not in the command are left unchanged.
/// </summary>
public sealed class UpdateTestimonialOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateTestimonialOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The testimonial id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success. Unknown ids in the list are silently skipped.</returns>
    public async Task<Result> HandleAsync(UpdateTestimonialOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var orderMap = cmd.Items.GroupBy(i => i.Id).ToDictionary(g => g.Key, g => g.Last().SortOrder);
        var ids = orderMap.Keys.ToList();
        var rows = await _context.Testimonials.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

        foreach (var row in rows)
        {
            row.SetSortOrder(orderMap[row.Id]);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
