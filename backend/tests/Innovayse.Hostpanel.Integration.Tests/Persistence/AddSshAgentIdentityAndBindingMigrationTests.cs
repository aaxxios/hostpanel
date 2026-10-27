using FluentAssertions;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary>
/// <c>AddSshAgentIdentityAndBinding</c> on a database that already holds servers, SSH settings and
/// SSH keys (the state production is in after the first SSH release): Up keeps every row and value
/// and leaves the two new columns empty; Down keeps every row and drops only those two columns;
/// Up again restores the columns, empty.
/// </summary>
public sealed class AddSshAgentIdentityAndBindingMigrationTests
{
    /// <summary>The migration before the one under test.</summary>
    private const string PreviousMigration = "20261027064149_AddSshKeyManagement";

    [Fact]
    public async Task UpDownUp_OnExistingSshData_KeepsEveryRowAndValueAsync()
    {
        var databaseName = $"it_{Guid.NewGuid():N}";
        var connectionString = await SharedPostgresServer.CreateDatabaseAsync(databaseName);
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;
            await using (var context = new AppDbContext(options))
            {
                await context.GetService<IMigrator>().MigrateAsync(PreviousMigration);
            }

            var (serverId, serviceId) = await SeedExistingStateAsync(options, connectionString);
            var before = await SnapshotAsync(connectionString);

            await using (var context = new AppDbContext(options))
            {
                await context.Database.MigrateAsync();
            }

            (await SnapshotAsync(connectionString)).Should().BeEquivalentTo(before, "Up adds columns and copies nothing");
            (await ScalarAsync(connectionString, "SELECT \"AgentBinding\" IS NULL AND \"AgentPublicKey\" IS NULL FROM \"ServerSshSettings\"")).Should().Be(true);

            // The upgraded model reads the old rows: an existing key keeps working, unbound.
            await using (var context = new AppDbContext(options))
            {
                // Projected, so the encrypted key column is not decrypted (it holds a placeholder here).
                var settings = await context.ServerSshSettings.AsNoTracking()
                    .Where(s => s.ServerId == serverId)
                    .Select(s => new { s.AgentBinding, s.IsEnabled })
                    .SingleAsync();
                settings.AgentBinding.Should().BeNull();
                settings.IsEnabled.Should().BeTrue();
                (await context.SshKeys.AsNoTracking().CountAsync(k => k.ClientServiceId == serviceId)).Should().Be(1);
            }

            await using (var context = new AppDbContext(options))
            {
                await context.GetService<IMigrator>().MigrateAsync(PreviousMigration);
            }

            (await SnapshotAsync(connectionString)).Should().BeEquivalentTo(before, "Down drops only the two new columns");
            (await ScalarAsync(connectionString, "SELECT count(*) FROM information_schema.columns WHERE table_name = 'ServerSshSettings' AND column_name IN ('AgentBinding','AgentPublicKey')")).Should().Be(0L);

            await using (var context = new AppDbContext(options))
            {
                await context.Database.MigrateAsync();
            }

            (await SnapshotAsync(connectionString)).Should().BeEquivalentTo(before, "Up again copies nothing either");
        }
        finally
        {
            using var probe = new NpgsqlConnection(connectionString);
            NpgsqlConnection.ClearPool(probe);
            await SharedPostgresServer.DropDatabaseAsync(databaseName);
        }
    }

    /// <summary>
    /// A server, an active service with one installed SSH key, and that server's SSH settings written
    /// in the pre-migration column set (raw SQL, as the current model has columns that do not exist yet).
    /// </summary>
    private static async Task<(int ServerId, int ServiceId)> SeedExistingStateAsync(DbContextOptions<AppDbContext> options, string connectionString)
    {
        await using var context = new AppDbContext(options);
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        var group = ProductGroup.Create($"mig-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        var server = Server.Create(new ServerDetails(
            "existing", "existing.example.test", null, null, ServerModule.Cwp7, "root", null, null, "hash", true, null,
            false, false, 0m, null, null, null, null, null, null, null, null, null, null, null, null));
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        context.Products.Add(product);
        await context.SaveChangesAsync();
        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        service.ActivateOn(server.Id, "web");
        context.ClientServices.Add(service);
        await context.SaveChangesAsync();

        var key = SshKey.Create(service.Id, server.Id, "web", "laptop", "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b", "SHA256:xK0G2nacQ5l2jswa38Dl9jmRCsESwmesIiz26XjE5+o");
        key.MarkInstalled();
        context.SshKeys.Add(key);
        await context.SaveChangesAsync();

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var insert = new NpgsqlCommand(
            "INSERT INTO \"ServerSshSettings\" (\"ServerId\",\"IsEnabled\",\"SshHostname\",\"SshPort\",\"AgentHost\",\"AgentPort\",\"AgentUsername\",\"AgentPrivateKey\",\"AgentHostKeyFingerprint\",\"ConnectTimeoutSeconds\",\"OperationTimeoutSeconds\",\"IsHealthy\") " +
            "VALUES (@s, true, 'existing.example.test', 2299, '10.0.0.9', 22, 'hpsshkeys', 'ENCRYPTED-BLOB', 'SHA256:V8WRk2pDzj4fSTJHYxEiHG4otIDHJcABkgUk2/Swhwk', 10, 20, true)", connection);
        insert.Parameters.AddWithValue("s", server.Id);
        await insert.ExecuteNonQueryAsync();
        return (server.Id, service.Id);
    }

    /// <summary>Every pre-existing value of the SSH tables, as text, for an exact before/after comparison.</summary>
    private static async Task<List<string>> SnapshotAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var rows = new List<string>();
        foreach (var sql in new[]
        {
            "SELECT \"ServerId\",\"IsEnabled\",\"SshHostname\",\"SshPort\",\"AgentHost\",\"AgentPort\",\"AgentUsername\",\"AgentPrivateKey\",\"AgentHostKeyFingerprint\",\"ConnectTimeoutSeconds\",\"OperationTimeoutSeconds\",\"IsHealthy\" FROM \"ServerSshSettings\" ORDER BY \"Id\"",
            "SELECT \"KeyId\",\"ClientServiceId\",\"ServerId\",\"AccountUsername\",\"Label\",\"PublicKey\",\"Fingerprint\",\"SyncState\" FROM \"SshKeys\" ORDER BY \"Id\"",
            "SELECT count(*) FROM \"Servers\"",
        })
        {
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? "<null>" : Convert.ToString(reader.GetValue(i)))));
            }
        }

        return rows;
    }

    /// <summary>Runs a scalar query.</summary>
    private static async Task<object?> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return await command.ExecuteScalarAsync();
    }
}
