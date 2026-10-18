using Innovayse.Hostpanel.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>
/// Query fragments over <see cref="Client"/> that more than one use case needs. A "my ..." use case
/// and an ownership check both begin by finding the client record behind the caller's token; this is
/// the one place that lookup is written.
/// </summary>
public static class ClientQueryExtensions
{
    /// <summary>Finds the client record linked to an identity subject, without change tracking.</summary>
    /// <param name="clients">The client set to search.</param>
    /// <param name="userId">The identity subject the client is linked to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The client, or <c>null</c> when the subject has no client record.</returns>
    public static Task<Client?> FindReadOnlyByUserIdAsync(
        this IQueryable<Client> clients, string userId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(clients);

        return clients.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    /// <summary>Eagerly loads the contacts and linked users of every client in the query.</summary>
    /// <param name="clients">The client query to load children for.</param>
    /// <returns>The query including both child collections.</returns>
    public static IQueryable<Client> IncludeChildren(this IQueryable<Client> clients)
    {
        ArgumentNullException.ThrowIfNull(clients);

        return clients
            .Include(c => c.Contacts)
            .Include(c => c.Users);
    }
}
