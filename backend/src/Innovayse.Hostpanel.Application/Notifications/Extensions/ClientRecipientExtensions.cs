using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Extensions;

/// <summary>
/// Resolves who a notification about a client should be mailed to. Every client-facing
/// notification event handler starts from this lookup, so the client-to-account hop is written once.
/// </summary>
public static class ClientRecipientExtensions
{
    /// <summary>Finds the sign-in address of the account behind a client record.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Resolves the client's account from the identity provider.</param>
    /// <param name="clientId">The client the notification is about.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The address, or <c>null</c> when the client does not exist or the identity provider does not
    /// know its account -- in which case there is nobody to notify.
    /// </returns>
    public static async Task<string?> FindClientEmailAsync(
        this IHostpanelDbContext context, IIdentityProvider identity, int clientId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(identity);

        var client = await context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId, ct);
        if (client is null)
        {
            return null;
        }

        var user = await identity.FindBySubjectAsync(client.UserId, ct);
        return user?.Email;
    }
}
