using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateBillableItem;

/// <summary>Creates a new billable item.</summary>
public sealed class CreateBillableItemHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus the owning modules' existence queries are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine bus the owning modules' existence queries are dispatched through.</param>
    public CreateBillableItemHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The create billable item command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the newly created billable item ID, or <c>CLIENT_NOT_FOUND</c> when the id names no client.</returns>
    public async Task<Result<int>> HandleAsync(CreateBillableItemCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (!await _bus.ClientExistsAsync(cmd.ClientId, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        var item = BillableItem.Create(
            cmd.ClientId, cmd.Description, cmd.Amount, cmd.Currency, cmd.Type, cmd.RecurringPeriod, cmd.NextDueDate);

        _context.BillableItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(item.Id);
    }
}
