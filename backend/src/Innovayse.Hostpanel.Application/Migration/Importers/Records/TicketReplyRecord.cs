namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One ticket reply as the source plugin describes it.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="TicketSubject">The ticket subject.</param>
/// <param name="Message">The message.</param>
/// <param name="AuthorName">The author name.</param>
/// <param name="IsStaffReply">Whether is staff reply.</param>
internal sealed record TicketReplyRecord(
    string ClientEmail, string TicketSubject, string Message, string AuthorName, bool IsStaffReply);
