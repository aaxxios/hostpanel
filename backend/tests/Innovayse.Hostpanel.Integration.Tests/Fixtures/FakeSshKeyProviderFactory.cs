using System.Collections.Concurrent;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// In-memory stand-in for every server's SSH key agent. One <see cref="FakeSshAgent"/> per server
/// id, created on first use, so tests that seed their own servers never see each other's keys and
/// two servers with the same Linux username keep separate key sets.
/// </summary>
public sealed class FakeSshKeyProviderFactory : ISshKeyProviderFactory
{
    /// <summary>Agents by server id.</summary>
    private readonly ConcurrentDictionary<int, FakeSshAgent> _agents = new();

    /// <summary>The agent of one server.</summary>
    /// <param name="serverId">The server.</param>
    /// <returns>Its fake agent.</returns>
    public FakeSshAgent AgentFor(int serverId) => _agents.GetOrAdd(serverId, id => new FakeSshAgent(id));

    /// <inheritdoc/>
    public bool Supports(ServerModule module) => module is ServerModule.Cwp7;

    /// <inheritdoc/>
    public ISshKeyProvider? CreateFor(Server server, ServerSshSettings settings) =>
        Supports(server.Module) ? AgentFor(server.Id) : null;
}

/// <summary>One server's key agent in memory, recording every call it receives.</summary>
public sealed class FakeSshAgent : ISshKeyProvider
{
    /// <summary>The server this agent runs on.</summary>
    private readonly int _serverId;

    /// <summary>Installed keys per account.</summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, string>> _accounts = new();

    /// <summary>Initialises the agent.</summary>
    /// <param name="serverId">The server this agent runs on.</param>
    public FakeSshAgent(int serverId)
    {
        _serverId = serverId;
    }

    /// <summary>Every call received, as "op:account".</summary>
    public ConcurrentQueue<string> Calls { get; } = new();

    /// <summary>When set, every call throws with this code.</summary>
    public string? FailWith { get; set; }

    /// <summary>Whether accounts have a login shell.</summary>
    public bool ShellEnabled { get; set; } = true;

    /// <summary>The keys installed for one account.</summary>
    /// <param name="username">The account.</param>
    /// <returns>Key ids to key lines.</returns>
    public IReadOnlyDictionary<Guid, string> KeysOf(string username) => Account(username);

    /// <inheritdoc/>
    public Task<string> PingAsync(CancellationToken ct)
    {
        Record("ping", null);
        return Task.FromResult("1.0.0");
    }

    /// <inheritdoc/>
    public Task<SshAccessStatus> GetAccessStatusAsync(SshAccount account, CancellationToken ct)
    {
        Record("status", account);
        return Task.FromResult(new SshAccessStatus(ShellEnabled, Account(account.Username).Count, 0));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<InstalledSshKey>> ListKeysAsync(SshAccount account, CancellationToken ct)
    {
        Record("list", account);
        IReadOnlyList<InstalledSshKey> keys = Account(account.Username).Keys.Select(k => new InstalledSshKey(k, "fp")).ToList();
        return Task.FromResult(keys);
    }

    /// <inheritdoc/>
    public Task AddKeyAsync(SshAccount account, Guid keyId, string publicKey, CancellationToken ct)
    {
        Record("add", account);
        Account(account.Username)[keyId] = publicKey;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RevokeKeyAsync(SshAccount account, Guid keyId, CancellationToken ct)
    {
        Record("revoke", account);
        Account(account.Username).TryRemove(keyId, out _);
        return Task.CompletedTask;
    }

    /// <summary>Records a call, refusing another server's account as the real provider does.</summary>
    /// <param name="op">Operation.</param>
    /// <param name="account">The account, if any.</param>
    private void Record(string op, SshAccount? account)
    {
        if (account is not null && account.ServerId != _serverId)
        {
            throw new InvalidOperationException($"Agent on server {_serverId} got an account of server {account.ServerId}.");
        }

        Calls.Enqueue($"{op}:{account?.Username}");
        if (FailWith is not null)
        {
            throw new SshProviderException(FailWith, "fake failure");
        }
    }

    /// <summary>The key set of one account, created on first use.</summary>
    /// <param name="username">The account.</param>
    /// <returns>The key set.</returns>
    private ConcurrentDictionary<Guid, string> Account(string username) => _accounts.GetOrAdd(username, _ => new());
}
