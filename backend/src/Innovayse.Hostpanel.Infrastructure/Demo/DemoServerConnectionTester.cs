using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>Reports every server as reachable without opening a connection.</summary>
internal sealed class DemoServerConnectionTester : IServerConnectionTester
{
    /// <summary>The account count the fake panel reports.</summary>
    private const int AccountsCount = 42;

    /// <summary>The version string the fake panel reports.</summary>
    private const string Version = "demo-1.0";

    /// <inheritdoc/>
    public Task<(bool Connected, int? AccountsCount, string? Version, string? ErrorMessage)> TestAsync(
        Server server, CancellationToken ct) =>
        Task.FromResult<(bool, int?, string?, string?)>((true, AccountsCount, Version, null));
}
