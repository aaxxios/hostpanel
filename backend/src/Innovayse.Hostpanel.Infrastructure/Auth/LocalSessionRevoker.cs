using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// The revoker for a deployment that owns its users: its tokens are stateless and it keeps no
/// refresh store, so there is nothing server-side to end.
/// </summary>
public sealed class LocalSessionRevoker : ISessionRevoker
{
    /// <inheritdoc/>
    public Task RevokeAllAsync(string subject, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
