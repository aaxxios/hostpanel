namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One support ticket as the source plugin describes it.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="Subject">The subject line; together with the client email it is the duplicate check.</param>
/// <param name="Message">The message body, as written.</param>
/// <param name="Status">The source's status word for the record, mapped to a local status by the importer's source-value table.</param>
/// <param name="Priority">The source's priority word, mapped to a local priority.</param>
/// <param name="DepartmentName">Department the ticket was filed under; matched to a local department by name.</param>
/// <param name="CreatedAt">When the source created the record (UTC); null when the source holds no date.</param>
internal sealed record TicketRecord(
    string ClientEmail, string Subject, string Message,
    string Status, string Priority, string? DepartmentName, DateTimeOffset? CreatedAt);
