namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>The feedback a client left on a ticket.</summary>
/// <param name="Rating">Client feedback rating (1-10), or null if not yet rated.</param>
/// <param name="Comment">Optional feedback comment from the client.</param>
/// <param name="LeftBy">Name of the client who left feedback.</param>
/// <param name="At">UTC timestamp when feedback was submitted.</param>
public sealed record TicketFeedbackDto(int? Rating, string? Comment, string? LeftBy, DateTimeOffset? At);
