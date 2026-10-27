using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Ends the client portal's sign-in sessions for a person. The portal keeps each session (its access
/// and refresh tokens) in Redis and lists the ids of one person's sessions in a set, so a back-channel
/// logout deletes every entry the set names and then the set.
/// </summary>
/// <remarks>
/// The key layout is a contract with <c>client/server/utils/session-store.ts</c>:
/// <c>hostpanel:portal-session:&lt;id&gt;</c> holds a session and
/// <c>hostpanel:portal-session-subject:&lt;sub&gt;</c> the set of its owner's ids. Change both together.
/// </remarks>
public sealed class PortalSessionRevoker : ISessionRevoker, IDisposable
{
    /// <summary>Prefix of the key one portal session is stored under.</summary>
    public const string SessionKeyPrefix = "hostpanel:portal-session:";

    /// <summary>Prefix of the key that lists the session ids of one person.</summary>
    public const string SubjectKeyPrefix = "hostpanel:portal-session-subject:";

    /// <summary>The connection, opened on first use so the API starts while Redis is still coming up.</summary>
    private readonly Lazy<Task<ConnectionMultiplexer>> _connection;

    /// <summary>Initializes a new instance of the <see cref="PortalSessionRevoker"/> class.</summary>
    /// <param name="options">Where the portal sessions are kept.</param>
    public PortalSessionRevoker(IOptions<PortalSessionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var configuration = ConfigurationOptions.Parse(options.Value.RedisConnection);
        // Keep retrying in the background instead of failing the first call while Redis restarts.
        configuration.AbortOnConnectFail = false;
        _connection = new Lazy<Task<ConnectionMultiplexer>>(() => ConnectionMultiplexer.ConnectAsync(configuration));
    }

    /// <inheritdoc/>
    public async Task RevokeAllAsync(string subject, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        var connection = await _connection.Value.WaitAsync(ct);
        var database = connection.GetDatabase();
        RedisKey subjectKey = SubjectKeyPrefix + subject;

        var ids = await database.SetMembersAsync(subjectKey).WaitAsync(ct);
        var keys = ids
            .Where(id => id.HasValue)
            .Select(id => (RedisKey)(SessionKeyPrefix + id.ToString()))
            .Append(subjectKey)
            .ToArray();
        await database.KeyDeleteAsync(keys).WaitAsync(ct);
    }

    /// <summary>Closes the connection when one was opened.</summary>
    public void Dispose()
    {
        if (_connection.IsValueCreated && _connection.Value.IsCompletedSuccessfully)
        {
            _connection.Value.Result.Dispose();
        }
    }
}
