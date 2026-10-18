using System.Text.Json;
using System.Text.Json.Serialization;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.SDK.Plugins;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Queries.ListPlugins;

/// <summary>
/// Handles <see cref="ListPluginsQuery"/> by scanning the plugins directory and
/// cross-referencing loaded status from <see cref="IPluginRegistry"/>.
/// </summary>
public sealed class ListPluginsHandler
{
    /// <summary>JSON options for deserialising plugin manifests.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Reports which of the plugins on disk were successfully loaded at startup.</summary>
    private readonly IPluginRegistry _registry;

    /// <summary>Initializes the handler.</summary>
    /// <param name="registry">Reports which of the plugins on disk were successfully loaded at startup.</param>
    public ListPluginsHandler(IPluginRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Scans the plugins directory and returns a summary list.
    /// </summary>
    /// <param name="query">The list query (no parameters).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every installed plugin, loaded or not, always succeeding.</returns>
    public async Task<Result<IReadOnlyList<PluginListItemDto>>> HandleAsync(
        ListPluginsQuery query, CancellationToken ct)
    {
        var pluginsRoot = Path.Combine(AppContext.BaseDirectory, "plugins");
        var result = new List<PluginListItemDto>();

        if (!Directory.Exists(pluginsRoot))
        {
            return Result<IReadOnlyList<PluginListItemDto>>.Success(result);
        }

        foreach (var dir in Directory.GetDirectories(pluginsRoot))
        {
            var manifestPath = Path.Combine(dir, "plugin.json");
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            try
            {
                var json = await File.ReadAllTextAsync(manifestPath, ct);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(json, JsonOptions);
                if (manifest is null)
                {
                    continue;
                }

                result.Add(new PluginListItemDto(
                    manifest.Id,
                    manifest.Name,
                    manifest.Version,
                    manifest.Author,
                    manifest.Description,
                    manifest.Type.ToString().ToLowerInvariant(),
                    manifest.Category,
                    manifest.Color,
                    _registry.IsLoaded(manifest.Id)));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                // Malformed manifest -- skip silently (logged at startup by PluginLoader).
            }
        }

        return Result<IReadOnlyList<PluginListItemDto>>.Success(result);
    }
}
