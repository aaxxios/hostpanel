using Innovayse.Hostpanel.Application.Clients.Common.Enums;

namespace Innovayse.Hostpanel.Application.Notifications.Common;

/// <summary>
/// Asks the mail sender to copy a message to the additional contacts of a client who switched
/// that kind of email on. A mail with no such request goes to its one recipient only.
/// </summary>
/// <param name="ClientId">The client whose contacts may receive a copy.</param>
/// <param name="Topic">The kind of email being sent; decides which contacts receive it.</param>
public sealed record ContactCopy(int ClientId, ContactNotification Topic);
