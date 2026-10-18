using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Migration.Queries.DownloadMigrationPlugin;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Public migration endpoints -- the legacy system being migrated away from downloads its
/// importer plugin from here, before it has any credential this platform would recognise.
/// </summary>
[Route("api/migrations")]
public sealed class MigrationImportController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public MigrationImportController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns the migration plugin as a downloadable ZIP archive.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the zipped plugin; 404 with <c>MIGRATION_PLUGIN_NOT_FOUND</c> when this
    /// deployment carries no plugin files.
    /// </returns>
    [HttpGet("plugin/download")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPluginAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<FileDownloadDto>>(
            new DownloadMigrationPluginQuery(), ct);
        return FileResult(outcome);
    }
}
