namespace Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartnerOrder;

/// <summary>Represents a single partner-to-sort-order mapping.</summary>
/// <param name="Id">The partner identifier.</param>
/// <param name="SortOrder">The new sort order value for the partner.</param>
public sealed record PartnerOrderItem(int Id, int SortOrder);
