namespace Innovayse.Hostpanel.Application.Notifications.Common;

/// <summary>
/// One email sent to the client, as the client's own list shows it. The mail server's delivery
/// error of <see cref="EmailLogDto"/> is for staff only and is not part of it.
/// </summary>
/// <param name="Id">The log entry's primary key.</param>
/// <param name="To">The address it was sent to.</param>
/// <param name="Subject">The subject line.</param>
/// <param name="SentAt">When it was sent.</param>
/// <param name="Success">Whether delivery to the mail server succeeded.</param>
public sealed record MyEmailLogDto(int Id, string To, string Subject, DateTimeOffset SentAt, bool Success);
