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

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreateMigrationJobHandler(IHostpanelDbContext context)
    {
        _context = context;
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

        var job = MigrationJob.Create(
            cmd.Label,
            sourceUrl,
            cmd.Selection.Clients,
            cmd.Selection.Invoices,
            cmd.Selection.Services,
            cmd.Selection.Domains,
            cmd.Selection.Tickets,
            cmd.Selection.Products,
            cmd.Selection.Orders,
            cmd.Selection.Transactions,
            cmd.Selection.Quotes,
            cmd.Selection.Knowledgebase,
            cmd.Selection.Contacts,
            cmd.Selection.TicketReplies,
            cmd.Selection.Announcements,
            cmd.Selection.Downloads,
            cmd.Selection.NetworkIssues);

        _context.MigrationJobs.Add(job);
        await _context.SaveChangesAsync(ct);

        return Result<MigrationJobDto>.Success(job.ToDto());
    }
}
