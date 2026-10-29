using Innovayse.Hostpanel.Domain.Billing.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>DTO representing a billable item in client-scoped views.</summary>
/// <param name="Id">Billable item primary key.</param>
/// <param name="ClientId">FK to the owning client.</param>
/// <param name="Description">Human-readable charge description.</param>
/// <param name="Amount">Charge amount.</param>
/// <param name="Currency">Currency code.</param>
/// <param name="Type">Item type.</param>
/// <param name="RecurringPeriod">Recurring period; null if one-time.</param>
/// <param name="IsInvoiced">Whether the item has been invoiced.</param>
/// <param name="InvoiceId">FK to invoice; null if uninvoiced.</param>
/// <param name="NextDueDate">Day the item is invoiced automatically (once for a one-time item, next for a recurring one), null for none.</param>
/// <param name="CreatedAt">Creation timestamp (UTC).</param>
public sealed record ClientBillableItemDto(
    int Id,
    int ClientId,
    string Description,
    decimal Amount,
    string Currency,
    BillableItemType Type,
    string? RecurringPeriod,
    bool IsInvoiced,
    int? InvoiceId,
    DateTimeOffset? NextDueDate,
    DateTimeOffset CreatedAt);
