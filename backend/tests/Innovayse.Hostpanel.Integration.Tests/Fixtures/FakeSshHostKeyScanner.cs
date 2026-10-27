using System.Collections.Concurrent;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// In-memory host key scanner: the key each host presents is set by the test; an unknown host is
/// unreachable. Hosts are unique per test, so tests never see each other's answers.
/// </summary>
public sealed class FakeSshHostKeyScanner : ISshHostKeyScanner
{
    /// <summary>Presented keys by "host:port".</summary>
    public ConcurrentDictionary<string, SshHostKeyScan> Keys { get; } = new();

    /// <inheritdoc/>
    public Task<SshHostKeyScan?> ScanAsync(string host, int port, TimeSpan timeout, CancellationToken ct) =>
        Task.FromResult(Keys.TryGetValue($"{host}:{port}", out var scan) ? scan : null);
}
