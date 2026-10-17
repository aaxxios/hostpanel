using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateTimeBillingEntries;

/// <summary>Creates billable items from time billing entries and persists them.</summary>
public sealed class CreateTimeBillingEntriesHandler
{
    /// <summary>The currency time-billed entries are charged in.</summary>
    private const string Currency = "USD";

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateTimeBillingEntriesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The create time billing entries command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the newly created billable item IDs. This use case refuses nothing itself.</returns>
    public async Task<Result<IReadOnlyList<int>>> HandleAsync(
        CreateTimeBillingEntriesCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var items = cmd.Entries
            .Select(entry => BillableItem.Create(
                cmd.ClientId, entry.Description, entry.Hours * entry.Rate, Currency, BillableItemType.OneTime))
            .ToList();

        _context.BillableItems.AddRange(items);
        await _context.SaveChangesAsync(ct);

        IReadOnlyList<int> ids = items.Select(x => x.Id).ToList();
        return Result<IReadOnlyList<int>>.Success(ids);
    }
}
