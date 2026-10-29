namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One ticket reply as the source plugin describes it.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="TicketSubject">Subject of the ticket the reply belongs to; with the client email it finds the imported ticket.</param>
/// <param name="Message">The message body, as written.</param>
/// <param name="AuthorName">Display name of whoever wrote the reply.</param>
/// <param name="IsStaffReply">Whether staff wrote the reply rather than the client.</param>
internal sealed record TicketReplyRecord(
    string ClientEmail, string TicketSubject, string Message, string AuthorName, bool IsStaffReply);
