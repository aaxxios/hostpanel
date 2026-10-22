using Npgsql;
using Testcontainers.PostgreSql;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// The one PostgreSQL server the whole integration run shares. <see cref="IntegrationTestFactory"/> used to
/// start a container of its own for every test class, so a full run launched several hundred servers and
/// the machine ran out of memory before the tests did (issue 373). Now a single container serves the
/// process and each factory takes a database of its own on it: the isolation a test class had before (an
/// empty, migrated, seeded database nobody else writes to) is unchanged, only the server underneath it is
/// shared.
/// </summary>
/// <remarks>
/// The container is disposed when the test process exits. The reaper sidecar would remove it too, but
/// the GitLab runner turns the reaper off, so the run cleans up after itself rather than leave a server
/// behind on the builder.
/// </remarks>
public static class SharedPostgresServer
{
    /// <summary>
    /// The connection limit the shared container is started with. Every factory's host keeps its own
    /// Npgsql pool, and tests that build a factory of their own add more beside the class fixtures, so
    /// the stock limit of 100 is too low for pools that were each sized for a server of their own.
    /// </summary>
    private const int MaxConnections = 1000;

    /// <summary>The server, started once on first use; every caller awaits the same task.</summary>
    private static readonly Lazy<Task<string>> Server = new(StartAsync);

    /// <summary>
    /// Creates an empty database on the shared server.
    /// </summary>
    /// <param name="databaseName">The unique name of the database to create.</param>
    /// <returns>The connection string of the new database.</returns>
    public static async Task<string> CreateDatabaseAsync(string databaseName)
    {
        var maintenance = await Server.Value;
        await using (var connection = new NpgsqlConnection(maintenance))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        return new NpgsqlConnectionStringBuilder(maintenance) { Database = databaseName }.ConnectionString;
    }

    /// <summary>
    /// Drops a database created by <see cref="CreateDatabaseAsync"/>, closing any session still on it.
    /// </summary>
    /// <param name="databaseName">The name of the database to drop.</param>
    /// <returns>A task that completes when the database is gone.</returns>
    public static async Task DropDatabaseAsync(string databaseName)
    {
        var maintenance = await Server.Value;
        await using var connection = new NpgsqlConnection(maintenance);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Starts the container and returns the connection string of its maintenance database.
    /// </summary>
    /// <returns>A connection string good for <c>CREATE DATABASE</c> and <c>DROP DATABASE</c>.</returns>
    private static async Task<string> StartAsync()
    {
        var container = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("innovayse_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCommand("-c", $"max_connections={MaxConnections}", "-c", "fsync=off")
            .Build();
        await container.StartAsync();

        // xUnit 2 has no assembly-level fixture to dispose a run-wide resource in, so the end of the
        // process is the one point that runs after the last test class has dropped its database.
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            container.DisposeAsync().AsTask().GetAwaiter().GetResult();
        };

        return container.GetConnectionString();
    }
}
