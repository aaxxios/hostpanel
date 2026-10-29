using Innovayse.Hostpanel.Application.Admin.Plugins.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Queries.ListPlugins;

/// <summary>
/// Handles <see cref="ListPluginsQuery"/> by reading the plugins on disk from
/// <see cref="IPluginPackageStore"/> and cross-referencing loaded status from <see cref="IPluginRegistry"/>.
/// </summary>
public sealed class ListPluginsHandler
{
    /// <summary>Reads the plugins on disk.</summary>
    private readonly IPluginPackageStore _store;

    /// <summary>Reports which of the plugins on disk were successfully loaded at startup.</summary>
    private readonly IPluginRegistry _registry;

    /// <summary>Initializes the handler.</summary>
    /// <param name="store">Reads the plugins on disk.</param>
    /// <param name="registry">Reports which of the plugins on disk were successfully loaded at startup.</param>
    public ListPluginsHandler(IPluginPackageStore store, IPluginRegistry registry)
    {
        _store = store;
        _registry = registry;
    }

    /// <summary>
    /// Returns every plugin on disk with its load status and whether it ships with the build.
    /// </summary>
    /// <param name="query">The list query (no parameters).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every plugin on disk, loaded or not, always succeeding.</returns>
    public async Task<Result<IReadOnlyList<PluginListItemDto>>> HandleAsync(
        ListPluginsQuery query, CancellationToken ct)
    {
        var packages = await _store.ListAsync(ct);
        var result = packages
            .Select(p => new PluginListItemDto(
                p.Manifest.Id,
                p.Manifest.Name,
                p.Manifest.Version,
                p.Manifest.Author,
                p.Manifest.Description,
                p.Manifest.Type.ToString().ToLowerInvariant(),
                p.Manifest.Category,
                p.Manifest.Color,
                _registry.IsLoaded(p.Manifest.Id),
                p.IsBuiltIn))
            .ToList();

        return Result<IReadOnlyList<PluginListItemDto>>.Success(result);
    }
}
