using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A configurable <see cref="IServerConnectionTester"/> standing in for a real network call to a
/// provisioning server. Answers connected with a fixed account count by default; setting
/// <see cref="ShouldFail"/> answers a failed connection instead, the way an unreachable server
/// would.
/// </summary>
public sealed class FakeServerConnectionTester : IServerConnectionTester
{
    /// <summary>
    /// When <see langword="true"/>, every call answers a failed connection instead of a
    /// successful one.
    /// </summary>
    public bool ShouldFail { get; set; }

    /// <summary>The account count a successful test reports.</summary>
    public int AccountsCount { get; set; } = 3;

    /// <summary>The servers this fake was asked to test, in call order.</summary>
    public List<Server> TestedServers { get; } = [];

    /// <inheritdoc/>
    public Task<(bool Connected, int? AccountsCount, string? Version, string? ErrorMessage)> TestAsync(
        Server server, CancellationToken ct)
    {
        TestedServers.Add(server);

        return ShouldFail
            ? Task.FromResult<(bool, int?, string?, string?)>((false, null, null, "fake connection refused"))
            : Task.FromResult<(bool, int?, string?, string?)>((true, AccountsCount, "fake-1.0", null));
    }
}
