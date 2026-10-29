using Innovayse.Hostpanel.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>
/// Query fragments over <see cref="Client"/> that more than one use case needs. The caller's own
/// account is resolved by <c>ICallerClientResolver</c>, never here.
/// </summary>
public static class ClientQueryExtensions
{
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
