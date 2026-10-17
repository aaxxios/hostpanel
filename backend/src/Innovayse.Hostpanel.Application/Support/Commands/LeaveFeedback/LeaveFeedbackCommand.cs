namespace Innovayse.Hostpanel.Application.Support.Commands.LeaveFeedback;

/// <summary>Records client feedback (rating + comment) for a ticket.</summary>
/// <param name="TicketId">The ticket identifier.</param>
/// <param name="Rating">Score from 1 to 10.</param>
/// <param name="Comment">Optional feedback comment.</param>
/// <param name="LeftBy">Display name of the client submitting the feedback.</param>
public sealed record LeaveFeedbackCommand(int TicketId, int Rating, string? Comment, string LeftBy);
