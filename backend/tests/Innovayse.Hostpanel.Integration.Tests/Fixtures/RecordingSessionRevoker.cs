using System.Collections.Concurrent;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Stands in for the session store behind <see cref="ISessionRevoker"/> (an external port) and
/// remembers which subjects were revoked, so a test can assert on it.
/// </summary>
public sealed class RecordingSessionRevoker : ISessionRevoker
{
    /// <summary>The subjects revoked so far, in call order.</summary>
    private readonly ConcurrentQueue<string> _revoked = new();

    /// <summary>Gets the subjects revoked so far.</summary>
    public IReadOnlyCollection<string> Revoked => _revoked.ToArray();

    /// <inheritdoc/>
    public Task RevokeAllAsync(string subject, CancellationToken ct)
    {
        _revoked.Enqueue(subject);
        return Task.CompletedTask;
    }
}
