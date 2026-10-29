using Innovayse.Hostpanel.Application.Clients.Common.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetContactNotificationEmails;

/// <summary>
/// Requests the email addresses of the additional contacts of one client who switched a kind of
/// email on, so the Notifications module can copy a message to them without reading the Clients
/// tables.
/// </summary>
/// <param name="ClientId">The client record whose contacts are asked about.</param>
/// <param name="Topic">The kind of email being sent; only contacts with that switch on are returned.</param>
public sealed record GetContactNotificationEmailsQuery(int ClientId, ContactNotification Topic);
