namespace Innovayse.Hostpanel.Domain.Support;

/// <summary>
/// Refusal codes the <see cref="Ticket"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so <c>ResourceCoverageTests</c> (which only reflects over
/// <c>HostpanelErrorCodes</c>) still requires a sentence for each one in every language.
/// </remarks>
public static class SupportErrors
{
    /// <summary>A closed ticket does not accept new replies (400) — <see cref="Ticket.AddReply"/>.</summary>
    public const string TicketReplyNotAllowed = "TICKET_REPLY_NOT_ALLOWED";

    /// <summary>The ticket is already closed (400) — <see cref="Ticket.Close"/>.</summary>
    public const string TicketCloseNotAllowed = "TICKET_CLOSE_NOT_ALLOWED";
}
