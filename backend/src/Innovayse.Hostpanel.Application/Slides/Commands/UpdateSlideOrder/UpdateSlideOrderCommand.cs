using Innovayse.Hostpanel.Application.Common;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlideOrder;

/// <summary>Command to update the display order of multiple slides in a single operation.</summary>
/// <param name="Items">The list of slide id and new sort order pairs.</param>
public sealed record UpdateSlideOrderCommand(List<SortOrderItem> Items);
