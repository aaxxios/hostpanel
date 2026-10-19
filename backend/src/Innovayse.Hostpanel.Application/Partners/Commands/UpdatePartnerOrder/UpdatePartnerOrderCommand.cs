namespace Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartnerOrder;

/// <summary>Command to update the display order of several partners in a single operation.</summary>
/// <param name="Items">The list of partner id and new sort order pairs.</param>
public sealed record UpdatePartnerOrderCommand(List<PartnerOrderItem> Items);
