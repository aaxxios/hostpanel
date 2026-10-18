namespace Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReplyCategory;

/// <summary>Command to permanently delete a predefined reply category.</summary>
/// <param name="Id">The category identifier. Overlaid from the route.</param>
public sealed record DeletePredefinedReplyCategoryCommand(int Id);
