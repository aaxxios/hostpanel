using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateBillableItem;

/// <summary>Creates a new billable item.</summary>
public sealed class CreateBillableItemHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateBillableItemHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The create billable item command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the newly created billable item ID. This use case refuses nothing itself.</returns>
    public async Task<Result<int>> HandleAsync(CreateBillableItemCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var type = Enum.Parse<BillableItemType>(cmd.Type);
        var item = BillableItem.Create(
            cmd.ClientId, cmd.Description, cmd.Amount, cmd.Currency, type, cmd.RecurringPeriod, cmd.NextDueDate);

        _context.BillableItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(item.Id);
    }
}
