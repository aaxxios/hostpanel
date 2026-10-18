namespace Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReply;

/// <summary>Command to permanently delete a predefined reply.</summary>
/// <param name="Id">The reply identifier. Overlaid from the route.</param>
public sealed record DeletePredefinedReplyCommand(int Id);
