#if INNOVAYSE_SSO
using Innovayse.Auth.Sessions.Interfaces;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Ends the cookie sessions (and the tokens behind them) that <c>Innovayse.Auth</c> keeps in
/// its session store for a person.
/// </summary>
public sealed class SsoSessionRevoker : ISessionRevoker
{
    /// <summary>The shared session store.</summary>
    private readonly ISessionStore _store;

    /// <summary>Initializes a new instance of the <see cref="SsoSessionRevoker"/> class.</summary>
    /// <param name="store">The shared session store.</param>
    public SsoSessionRevoker(ISessionStore store)
    {
        _store = store;
    }

    /// <inheritdoc/>
    public Task RevokeAllAsync(string subject, CancellationToken ct)
    {
        return _store.DeleteAllForUserAsync(subject, ct);
    }
}
#endif
