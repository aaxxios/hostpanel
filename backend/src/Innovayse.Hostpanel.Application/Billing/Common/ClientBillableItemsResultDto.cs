using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Billing.Common;
/// <summary>Combined result of uninvoiced and invoiced billable items for a client profile view.</summary>
/// <param name="UninvoicedItems">All uninvoiced billable items for the client.</param>
/// <param name="UninvoicedTotal">Sum of all uninvoiced item amounts.</param>
/// <param name="InvoicedItems">Paginated list of invoiced billable items.</param>
public sealed record ClientBillableItemsResultDto(
    IReadOnlyList<ClientBillableItemDto> UninvoicedItems,
    decimal UninvoicedTotal,
    PaginatedList<ClientBillableItemDto> InvoicedItems);
