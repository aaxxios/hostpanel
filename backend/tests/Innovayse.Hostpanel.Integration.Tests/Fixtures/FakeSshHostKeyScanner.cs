using System.Collections.Concurrent;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// In-memory host key scanner: the key each host presents, or how it fails, is set by the test; an
/// unknown host gives no answer. Hosts are unique per test, so tests never see each other's answers.
/// </summary>
public sealed class FakeSshHostKeyScanner : ISshHostKeyScanner
{
    /// <summary>Presented keys by "host:port".</summary>
    public ConcurrentDictionary<string, SshHostKeyScan> Keys { get; } = new();

    /// <summary>How each "host:port" fails, when it presents no key.</summary>
    public ConcurrentDictionary<string, SshEndpointFailure> Failures { get; } = new();

    /// <inheritdoc/>
    public Task<SshHostKeyProbe> ScanAsync(SshEndpoint endpoint, CancellationToken ct)
    {
        var address = $"{endpoint.Host}:{endpoint.Port}";
        return Task.FromResult(Keys.TryGetValue(address, out var scan)
            ? new SshHostKeyProbe(scan, null)
            : new SshHostKeyProbe(null, Failures.TryGetValue(address, out var failure) ? failure : SshEndpointFailure.NoAnswer));
    }
}
