namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteNetworkIssue;

/// <summary>Command to permanently delete a network issue.</summary>
/// <param name="Id">The network issue identifier.</param>
public sealed record DeleteNetworkIssueCommand(int Id);
