using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Looks up the Innovayse <c>Client.Id</c> for a plugin record's email address. Extracted from
/// <c>MigrationPullWorker</c> (A6 Task 6) so every importer that needs to resolve a client shares
/// the same lookup rather than repeating it.
/// </summary>
/// <param name="identity">Resolves the plugin's email address to this platform's own identity.</param>
/// <param name="context">The module's data context, for mapping the resolved identity to its client row.</param>
public sealed class MigrationClientResolver(IIdentityProvider identity, IHostpanelDbContext context)
{
    /// <summary>
    /// Resolves <paramref name="email"/> to the local client it belongs to, if any.
    /// </summary>
    /// <param name="email">The email address as the source plugin recorded it.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching client's id, or <see langword="null"/> when no identity account or no client
    /// row exists for that email.
    /// </returns>
    public async Task<int?> ResolveClientIdAsync(string email, CancellationToken ct)
    {
        var user = await identity.FindByEmailAsync(email, ct);
        if (user is null)
        {
            return null;
        }

        var client = await context.Clients
            .IncludeChildren()
            .FirstOrDefaultAsync(c => c.UserId == user.Subject, ct);
        return client?.Id;
    }
}
