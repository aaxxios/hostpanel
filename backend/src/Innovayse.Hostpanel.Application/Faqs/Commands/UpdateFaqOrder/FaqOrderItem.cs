namespace Innovayse.Hostpanel.Application.Faqs.Commands.UpdateFaqOrder;

/// <summary>Represents a single question-to-sort-order mapping.</summary>
/// <param name="Id">The question identifier.</param>
/// <param name="SortOrder">The new sort order value for the question.</param>
public sealed record FaqOrderItem(int Id, int SortOrder);
