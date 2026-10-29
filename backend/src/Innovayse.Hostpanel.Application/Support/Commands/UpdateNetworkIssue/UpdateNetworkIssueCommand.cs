using Innovayse.Hostpanel.Domain.Support.Enums;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateNetworkIssue;

/// <summary>Command to update an existing network issue.</summary>
/// <param name="Id">The network issue identifier; nullable so the route id can overlay the body.</param>
/// <param name="Title">The new issue title.</param>
/// <param name="Type">The new type.</param>
/// <param name="Server">The new affected server name, or null.</param>
/// <param name="Priority">The new priority level.</param>
/// <param name="Status">The new lifecycle status.</param>
/// <param name="StartDate">The new start date.</param>
/// <param name="EndDate">The new end date, or null.</param>
/// <param name="Description">The new HTML description.</param>
public sealed record UpdateNetworkIssueCommand(
    int? Id,
    string Title,
    NetworkIssueType Type,
    string? Server,
    NetworkIssuePriority Priority,
    NetworkIssueStatus Status,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    string Description);
