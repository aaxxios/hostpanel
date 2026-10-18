namespace Innovayse.Hostpanel.Application.Reports.Queries.GetTicketTags;

/// <summary>Ticket Tags report result.</summary>
/// <param name="Rows">One entry per distinct tag used in the period.</param>
public record TicketTagsDto(IReadOnlyList<TicketTagRowDto> Rows);
