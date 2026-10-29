namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>
/// Holds the Name.am access token once per registrar connection for the whole process.
/// <see cref="NameAmClient"/> is created per request, so a token kept on it would be thrown away
/// after every call and each call would log in again; the one token here is shared instead.
/// </summary>
/// <remarks>
/// Every read and write of a token happens inside one gate, and a login runs inside it too, so
/// concurrent callers that find no token wait for the single login instead of each starting their
/// own. A connection is the API address, the account e-mail and the test mode flag: a token issued
/// for one of them is never offered to another.
/// </remarks>
public sealed class NameAmTokenStore
{
    /// <summary>Serialises token reads, logins and replacements.</summary>
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>The cached token for each connection; touched only while holding <see cref="_gate"/>.</summary>
    private readonly Dictionary<string, string> _tokens = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the cached token for a connection, logging in once when there is none.
    /// </summary>
    /// <param name="connection">The connection the token belongs to.</param>
    /// <param name="login">Logs in and returns a fresh token; called at most once, inside the gate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The access token.</returns>
    public Task<string> GetAsync(
        string connection, Func<CancellationToken, Task<string>> login, CancellationToken ct)
    {
        return RefreshAsync(connection, null, login, ct);
    }

    /// <summary>
    /// Replaces a token Name.am rejected, or makes the first one when none is cached. When another
    /// caller has already replaced the rejected token, that newer token is returned and no second
    /// login is made.
    /// </summary>
    /// <param name="connection">The connection the token belongs to.</param>
    /// <param name="rejected">The token that was answered with 401, or <see langword="null"/> when none was used yet.</param>
    /// <param name="login">Logs in and returns a fresh token; called at most once, inside the gate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A token that is not the rejected one.</returns>
    public async Task<string> RefreshAsync(
        string connection, string? rejected, Func<CancellationToken, Task<string>> login, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_tokens.TryGetValue(connection, out var cached) && !string.Equals(cached, rejected, StringComparison.Ordinal))
            {
                return cached;
            }

            var token = await login(ct);
            _tokens[connection] = token;
            return token;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Logs in regardless of any cached token and caches the result, so a credentials check
    /// reflects what Name.am accepts now and later calls reuse the token it issued.
    /// </summary>
    /// <param name="connection">The connection the token belongs to.</param>
    /// <param name="login">Logs in and returns a fresh token; called once, inside the gate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the new token is cached.</returns>
    public async Task ReplaceAsync(
        string connection, Func<CancellationToken, Task<string>> login, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            _tokens[connection] = await login(ct);
        }
        finally
        {
            _gate.Release();
        }
    }
}
