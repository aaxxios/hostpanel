using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteBillableItem;

/// <summary>Deletes a billable item.</summary>
public sealed class DeleteBillableItemHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteBillableItemHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The delete billable item command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>BILLABLE_ITEM_NOT_FOUND</c> when the id names no billable item.</returns>
    public async Task<Result> HandleAsync(DeleteBillableItemCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var item = await _context.BillableItems.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (item is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.BILLABLE_ITEM_NOT_FOUND);
        }

        _context.BillableItems.Remove(item);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
