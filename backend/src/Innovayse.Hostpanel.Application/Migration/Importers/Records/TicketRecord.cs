namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One support ticket as the source plugin describes it.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="Subject">The subject.</param>
/// <param name="Message">The message.</param>
/// <param name="Status">The status.</param>
/// <param name="Priority">The priority.</param>
/// <param name="DepartmentName">The department name.</param>
/// <param name="CreatedAt">The created at.</param>
internal sealed record TicketRecord(
    string ClientEmail, string Subject, string Message,
    string Status, string Priority, string? DepartmentName, DateTimeOffset CreatedAt);
