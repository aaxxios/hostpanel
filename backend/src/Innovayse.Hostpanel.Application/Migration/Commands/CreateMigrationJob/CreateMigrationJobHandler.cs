using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Migration.Commands.CreateMigrationJob;

/// <summary>
/// Creates a new pending migration job, generating the secret key the migration plugin will
/// authenticate its pull requests with.
/// </summary>
public sealed class CreateMigrationJobHandler
{
    /// <summary>The path every source plugin listens on, appended when the operator's URL omits it.</summary>
    private const string PluginPath = "/modules/addons/innovayse_migration/api.php";

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public CreateMigrationJobHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The source and the entity types to export from it.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the created job. Never refuses.</returns>
    public async Task<Result<MigrationJobDto>> HandleAsync(CreateMigrationJobCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var sourceUrl = cmd.SourceUrl.TrimEnd('/');
        if (!sourceUrl.EndsWith(PluginPath, StringComparison.OrdinalIgnoreCase))
        {
            sourceUrl += PluginPath;
        }

        var selection = cmd.Selection;
        var job = MigrationJob.Create(new MigrationJobSetup(
            cmd.Label,
            sourceUrl,
            new MigrationExportSelection(
                selection.Clients,
                selection.Invoices,
                selection.Services,
                selection.Domains,
                selection.Tickets,
                selection.Products,
                selection.Orders,
                selection.Transactions,
                selection.Quotes,
                selection.Knowledgebase,
                selection.Contacts,
                selection.TicketReplies,
                selection.Announcements,
                selection.Downloads,
                selection.NetworkIssues)));

        _context.MigrationJobs.Add(job);
        await _context.SaveChangesAsync(ct);

        return Result<MigrationJobDto>.Success(job.ToDto(_clock.UtcNow));
    }
}
