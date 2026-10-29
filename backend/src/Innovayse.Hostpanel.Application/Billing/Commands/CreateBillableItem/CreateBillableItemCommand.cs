using Innovayse.Hostpanel.Domain.Billing.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateBillableItem;

/// <summary>Command to create a new billable item.</summary>
/// <param name="ClientId">
/// FK to the client. Overlaid from the route when created under a client profile.
/// </param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Amount">Unit price.</param>
/// <param name="Currency">Currency code (e.g. USD).</param>
/// <param name="Type">Item type.</param>
/// <param name="RecurringPeriod">Recurring period (Monthly, Quarterly, Annual): required for Recurring.</param>
/// <param name="NextDueDate">
/// Optional day the item is invoiced automatically: once for a one-time item, next for a recurring
/// one. Null leaves the item to be invoiced by hand.
/// </param>
public sealed record CreateBillableItemCommand(
    int ClientId,
    string Description,
    decimal Amount,
    string Currency,
    BillableItemType Type,
    string? RecurringPeriod = null,
    DateTimeOffset? NextDueDate = null);
