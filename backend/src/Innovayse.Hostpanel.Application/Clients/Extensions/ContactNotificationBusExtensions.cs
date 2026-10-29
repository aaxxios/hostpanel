using Innovayse.Hostpanel.Application.Clients.Common.Enums;
using Innovayse.Hostpanel.Application.Clients.Queries.GetContactNotificationEmails;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>
/// The shape other modules ask <see cref="GetContactNotificationEmailsQuery"/> in, so a sender
/// copies a message to a client's contacts without reading the Clients tables.
/// </summary>
public static class ContactNotificationBusExtensions
{
    /// <summary>The addresses of a client's additional contacts who want a kind of email.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the query through.</param>
    /// <param name="clientId">The client record's id.</param>
    /// <param name="topic">The kind of email being sent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The distinct addresses, none when no contact wants that kind of email.</returns>
    public static async Task<IReadOnlyList<string>> ListContactNotificationEmailsAsync(
        this IMessageBus bus, int clientId, ContactNotification topic, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<string>>>(new GetContactNotificationEmailsQuery(clientId, topic), ct);

        return result.Value ?? [];
    }
}
