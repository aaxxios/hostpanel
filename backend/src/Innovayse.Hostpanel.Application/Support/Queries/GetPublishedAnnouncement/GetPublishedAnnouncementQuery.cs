namespace Innovayse.Hostpanel.Application.Support.Queries.GetPublishedAnnouncement;

/// <summary>Query to retrieve one published announcement by its identifier.</summary>
/// <remarks>
/// Carries no "include unpublished" switch, for the same reason the list does not: which rows are
/// visible is settled by the message type. The admin read is a separate use case,
/// <c>GetAnnouncementQuery</c>.
/// </remarks>
/// <param name="Id">The announcement identifier.</param>
public sealed record GetPublishedAnnouncementQuery(int Id);
