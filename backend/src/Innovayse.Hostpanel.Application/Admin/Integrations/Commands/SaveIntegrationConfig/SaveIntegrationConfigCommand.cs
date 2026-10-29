using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Commands.SaveIntegrationConfig;

/// <summary>
/// Command that upserts all configuration settings for one integration.
/// </summary>
/// <param name="Slug">
/// URL-safe integration identifier, e.g. "card". The controller overlays it from the route
/// (<c>command with { Slug = slug }</c>); the admin panel's body carries only <paramref name="IsEnabled"/> and <paramref name="Config"/>.
/// </param>
/// <param name="IsEnabled">Whether the integration should be active after saving.</param>
/// <param name="Config">
/// Dictionary of field key to value to persist. A secret field with an empty or absent value keeps
/// what is stored: the admin panel never receives a secret, so it has nothing to send back.
/// </param>
/// <param name="ClearFields">
/// Field keys whose stored value is removed. The explicit list is what tells "clear this secret"
/// apart from "leave it as it is", which an empty value means for a secret.
/// </param>
public sealed record SaveIntegrationConfigCommand(
    string Slug,
    bool IsEnabled,
    Dictionary<string, string?> Config,
    IReadOnlyList<string>? ClearFields = null) : IDemoRestricted;
