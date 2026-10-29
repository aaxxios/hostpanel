namespace Innovayse.Hostpanel.Domain.Servers;

/// <summary>
/// The proportional fill rule that decides which server receives the next hosting account.
/// <para>
/// 1. Disabled servers and servers at full capacity are excluded.
/// 2. The server with the lowest fill ratio (accounts / maximum accounts) wins.
/// 3. A tie on the ratio goes to the server with the higher maximum capacity.
/// 4. A server with no capacity limit counts as having unlimited capacity and a 0% fill.
/// 5. A server whose account count is unknown counts as empty.
/// </para>
/// A tie on both keys keeps the order the candidates were given in.
/// </summary>
public static class ServerFillPolicy
{
    /// <summary>
    /// Picks the server that should receive the next account.
    /// </summary>
    /// <param name="servers">Candidate servers, in the order that settles a full tie.</param>
    /// <returns>The chosen server, or <see langword="null"/> when no candidate is eligible.</returns>
    public static Server? SelectBest(IEnumerable<Server> servers)
    {
        // OrderBy is a stable sort, so the caller's order decides between two servers that tie on both keys.
        return servers
            .Where(s => !s.IsDisabled)
            .Where(s => !IsFull(s))
            .OrderBy(FillRatio)
            .ThenByDescending(s => s.MaxAccounts ?? int.MaxValue)
            .FirstOrDefault();
    }

    /// <summary>
    /// Calculates how full a server is.
    /// </summary>
    /// <param name="server">The server to measure.</param>
    /// <returns>The fill ratio between 0.0 and 1.0; 0.0 for a server with no capacity limit.</returns>
    public static double FillRatio(Server server)
    {
        if (server.MaxAccounts is null or 0)
        {
            return 0.0;
        }

        var accounts = server.AccountsCount ?? 0;
        return (double)accounts / server.MaxAccounts.Value;
    }

    /// <summary>
    /// Checks whether a server can take no more accounts.
    /// </summary>
    /// <param name="server">The server to check.</param>
    /// <returns><see langword="true"/> when the server has a limit and has reached it.</returns>
    public static bool IsFull(Server server)
    {
        if (server.MaxAccounts is null)
        {
            return false;
        }

        return (server.AccountsCount ?? 0) >= server.MaxAccounts.Value;
    }
}
