using Innovayse.Hostpanel.Application.Admin.Seeding;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Seeding;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the public demo's sample data and wipes it on a schedule. Seeding runs the per-concern
/// seeders in dependency order; resetting truncates every table in the model except the identity
/// roles and the migration-seeded reference tables, then seeds again, all inside one transaction so a failure leaves yesterday's data
/// standing instead of an empty database.
/// </summary>
internal sealed class DemoDataManager : IDemoDataManager
{
    /// <summary>Identity role tables kept across a reset: roles are configuration, not demo data. Tables seeded by migrations (<c>HasData</c>) are kept too, derived from the model.</summary>
    private static readonly HashSet<string> KeptTables = ["AspNetRoles", "AspNetRoleClaims"];

    /// <summary>The data context the seeders write through and the reset truncates.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the demo accounts.</summary>
    private readonly UserManager<AppUser> _users;

    /// <summary>Discards the domain events the seeded rows would raise.</summary>
    private readonly IDomainEventGate _eventGate;

    /// <summary>Unit of work the shared deployment seeders save through.</summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>The demo switch and the published accounts.</summary>
    private readonly IOptions<DemoOptions> _options;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<DemoDataManager> _logger;

    /// <summary>Creates the manager.</summary>
    /// <param name="db">The data context the seeders write through and the reset truncates.</param>
    /// <param name="users">Creates the demo accounts.</param>
    /// <param name="eventGate">Discards the domain events the seeded rows would raise.</param>
    /// <param name="unitOfWork">Unit of work the shared deployment seeders save through.</param>
    /// <param name="options">The demo switch and the published accounts.</param>
    /// <param name="logger">Structured logger.</param>
    public DemoDataManager(
        AppDbContext db,
        UserManager<AppUser> users,
        IDomainEventGate eventGate,
        IUnitOfWork unitOfWork,
        IOptions<DemoOptions> options,
        ILogger<DemoDataManager> logger)
    {
        _db = db;
        _users = users;
        _eventGate = eventGate;
        _unitOfWork = unitOfWork;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SeedAsync(CancellationToken ct)
    {
        if (!_options.Value.Enabled)
        {
            return;
        }

        // Seeded rows are fixtures, not business events: no welcome mails, no payment receipts.
        using var suppression = _eventGate.Suppress();
        await SeedAllAsync(ct);
        _logger.LogInformation("Demo data seeded.");
    }

    /// <inheritdoc/>
    public async Task ResetAsync(CancellationToken ct)
    {
        if (!_options.Value.Enabled)
        {
            return;
        }

        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            using var suppression = _eventGate.Suppress();

            // Drop whatever the previous run (or a visitor) left tracked; the rows are about to go.
            _db.ChangeTracker.Clear();
            await TruncateAsync(ct);
            await SeedAllAsync(ct);

            await transaction.CommitAsync(ct);
        });
    }

    /// <summary>Runs every seeder in dependency order.</summary>
    /// <param name="ct">Cancels the seed.</param>
    /// <returns>A task that completes once everything is saved.</returns>
    private async Task SeedAllAsync(CancellationToken ct)
    {
        // The two deployment-wide seeders every environment runs; both are idempotent and a reset
        // has just emptied the tables they fill.
        await DefaultDepartmentsSeeder.EnsureSeededAsync(_db, _unitOfWork, ct);
        await PortalSettingsSeeder.EnsureSeededAsync(_db, _unitOfWork, ct);

        await new DemoCatalogueSeeder(_db).SeedAsync(ct);
        var demoClientId = await new DemoPeopleSeeder(_db, _users, _options.Value).SeedAsync(ct);
        await new DemoBillingSeeder(_db).SeedAsync(ct);
        await new DemoSupportSeeder(_db).SeedAsync(demoClientId, ct);
        await new DemoServerSeeder(_db).SeedAsync(ct);
    }

    /// <summary>Empties every mapped table except the kept identity role tables and the tables the migrations seed through <c>HasData</c> (email templates among them), which nothing would restore.</summary>
    /// <param name="ct">Cancels the statement.</param>
    /// <returns>A task that completes once the tables are empty.</returns>
    private async Task TruncateAsync(CancellationToken ct)
    {
        // The design-time model, not _db.Model: the runtime model is read-optimized and refuses
        // GetSeedData, which is exactly how the migrations' HasData rows are told apart here.
        var tables = _db.GetService<IDesignTimeModel>().Model.GetEntityTypes()
            .Where(t => t.GetTableName() is not null
                && !KeptTables.Contains(t.GetTableName()!)
                && !t.GetSeedData().Any())
            .Select(t => (Schema: t.GetSchema() ?? t.Model.GetDefaultSchema(), Table: t.GetTableName()!))
            .Distinct()
            .Select(t => t.Schema is null ? $"\"{t.Table}\"" : $"\"{t.Schema}\".\"{t.Table}\"")
            .ToList();

        // ExecuteSqlRaw is used because TRUNCATE has no parameter or LINQ form and the table list is
        // only known from the model at runtime. Every identifier is quoted and comes from the EF
        // model's table and schema names, never from a request, so nothing here is injectable.
        // CASCADE for the foreign keys between the listed tables; RESTART IDENTITY so ids repeat
        // after every reset and the demo's URLs stay stable.
#pragma warning disable EF1002 // The identifiers come from the EF model, never from a request.
        await _db.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {string.Join(", ", tables)} RESTART IDENTITY CASCADE", ct);
#pragma warning restore EF1002
    }
}
