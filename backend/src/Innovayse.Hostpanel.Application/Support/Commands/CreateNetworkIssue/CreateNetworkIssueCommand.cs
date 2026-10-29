using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateNetworkIssue;

/// <summary>Command to create a new network issue.</summary>
/// <param name="Title">The issue title.</param>
/// <param name="Type">The type of network issue.</param>
/// <param name="Server">The affected server name, or null.</param>
/// <param name="Priority">The priority level.</param>
/// <param name="StartDate">UTC timestamp when the issue started.</param>
/// <param name="Description">HTML description of the issue.</param>
public sealed record CreateNetworkIssueCommand(
    string Title,
    NetworkIssueType Type,
    string? Server,
    NetworkIssuePriority Priority,
    DateTimeOffset StartDate,
    string Description);
