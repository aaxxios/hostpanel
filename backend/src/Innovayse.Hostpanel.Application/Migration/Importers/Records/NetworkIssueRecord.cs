namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One network issue as the source plugin describes it.</summary>
/// <param name="Title">Issue headline shown on the network status page.</param>
/// <param name="Type">The source's word for what is affected (server, system, other).</param>
/// <param name="Status">The source's status word for the record, mapped to a local status by the importer's source-value table.</param>
/// <param name="Priority">The source's priority word, mapped to a local priority.</param>
/// <param name="Description">Longer descriptive text; null or empty when the source holds none.</param>
/// <param name="StartDate">When the issue started.</param>
/// <param name="EndDate">When the issue was resolved; null while it is open.</param>
internal sealed record NetworkIssueRecord(
    string Title, string Type, string Status, string Priority,
    string Description, DateTimeOffset? StartDate, DateTimeOffset? EndDate);
