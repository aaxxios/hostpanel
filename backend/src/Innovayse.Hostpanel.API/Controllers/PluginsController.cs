using Innovayse.Hostpanel.Application.Admin.Plugins.Commands.InstallPlugin;
using Innovayse.Hostpanel.Application.Admin.Plugins.Commands.RemovePlugin;
using Innovayse.Hostpanel.Application.Admin.Plugins.Common;
using Innovayse.Hostpanel.Application.Admin.Plugins.Queries.ListPlugins;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Staff administration of installable provider plugins. Every write answers
/// <c>requiresRestart: true</c> -- the client must call <see cref="Restart"/> before changes take
/// effect.
/// </summary>
[Route("api/admin/plugins")]
[Authorize(Policy = Policies.Admin)]
public sealed class PluginsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Host lifetime used to trigger a graceful restart after a plugin change.</summary>
    private readonly IHostApplicationLifetime _lifetime;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    /// <param name="lifetime">Host lifetime used to trigger a graceful restart after a plugin change.</param>
    public PluginsController(IMessageBus bus, IHostApplicationLifetime lifetime)
    {
        _bus = bus;
        _lifetime = lifetime;
    }

    /// <summary>Every plugin installed on disk, with its load status.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the plugins; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PluginListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<PluginListItemDto>>>(new ListPluginsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Installs a plugin from an uploaded ZIP archive.</summary>
    /// <param name="file">The uploaded ZIP file. <c>IFormFile</c> cannot be a command member, so its
    /// stream and length are passed to the command, which does the reading.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>requiresRestart: true</c> on success; 400 with <c>PLUGIN_MANIFEST_INVALID</c>,
    /// <c>PLUGIN_SDK_VERSION_UNSUPPORTED</c> or <c>PLUGIN_ARCHIVE_UNSAFE</c>; 401; 403.
    /// </returns>
    [HttpPost("install")]
    [RequestSizeLimit(52_428_800)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PluginActionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InstallAsync(IFormFile file, CancellationToken ct)
    {
        await using var content = file?.OpenReadStream() ?? Stream.Null;
        var outcome = await _bus.InvokeAsync<Result<PluginActionResultDto>>(
            new InstallPluginCommand(content, file?.Length ?? 0), ct);
        return HandleResult(outcome);
    }

    /// <summary>Removes an installed plugin by deleting its files from disk.</summary>
    /// <param name="id">Plugin identifier as declared in <c>plugin.json</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>requiresRestart: true</c> on success; 404 with <c>PLUGIN_NOT_FOUND</c>; 400 with
    /// <c>PLUGIN_ID_INVALID</c>; 401; 403.
    /// </returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(PluginActionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAsync(string id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PluginActionResultDto>>(new RemovePluginCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Triggers a graceful application restart so newly installed or removed plugins take effect.
    /// The process manager (systemd, Docker restart policy) brings the process back up. Not a use
    /// case of its own -- there is no state to load or save, only the host's own shutdown.
    /// </summary>
    /// <returns>202 with no body; 401; 403.</returns>
    [HttpPost("restart")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult Restart()
    {
        _lifetime.StopApplication();
        return AcceptedResult(Result.Success());
    }
}
