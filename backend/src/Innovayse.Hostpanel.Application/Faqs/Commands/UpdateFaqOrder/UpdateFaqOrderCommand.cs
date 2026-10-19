namespace Innovayse.Hostpanel.Application.Faqs.Commands.UpdateFaqOrder;

/// <summary>Command to update the display order of several questions in a single operation.</summary>
/// <param name="Items">The list of question id and new sort order pairs.</param>
public sealed record UpdateFaqOrderCommand(List<FaqOrderItem> Items);
