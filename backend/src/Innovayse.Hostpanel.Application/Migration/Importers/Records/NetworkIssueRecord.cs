namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One network issue as the source plugin describes it.</summary>
/// <param name="Title">The title.</param>
/// <param name="Type">The type.</param>
/// <param name="Status">The status.</param>
/// <param name="Priority">The priority.</param>
/// <param name="Description">The description.</param>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
internal sealed record NetworkIssueRecord(
    string Title, string Type, string Status, string Priority,
    string Description, DateTimeOffset? StartDate, DateTimeOffset? EndDate);
