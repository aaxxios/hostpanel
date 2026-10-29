using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// The revoker for a deployment that owns its users. Its access tokens are stateless, but each
/// one carries the session version it was issued with (see <see cref="SessionVersion"/>), so
/// changing the account's security stamp ends every session issued before.
/// </summary>
public sealed class LocalSessionRevoker : ISessionRevoker
{
    /// <summary>The Identity user manager, which owns the security stamp.</summary>
    private readonly UserManager<AppUser> _users;

    /// <summary>Initializes a new instance of the <see cref="LocalSessionRevoker"/> class.</summary>
    /// <param name="users">The Identity user manager, which owns the security stamp.</param>
    public LocalSessionRevoker(UserManager<AppUser> users)
    {
        _users = users;
    }

    /// <inheritdoc/>
    public async Task RevokeAllAsync(string subject, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return;
        }

        await _users.UpdateSecurityStampAsync(user);
    }
}
