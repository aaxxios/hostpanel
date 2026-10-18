using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Commands.SaveIntegrationConfig;

/// <summary>
/// Command that upserts all configuration settings for one integration.
/// </summary>
/// <param name="Slug">
/// URL-safe integration identifier, e.g. "card". Nullable because the controller overlays it
/// from the route (<c>command with { Slug = slug }</c>): a non-nullable member of a bound body is
/// implicitly required, so the request would be refused before the overlay could run -- the admin
/// panel's body carries only <paramref name="IsEnabled"/> and <paramref name="Config"/>.
/// </param>
/// <param name="IsEnabled">Whether the integration should be active after saving.</param>
/// <param name="Config">
/// Dictionary of field key to value to persist.
/// Fields whose value equals the mask placeholder ("••••••••") are skipped so stored
/// secrets are not overwritten when the admin re-saves without changing them.
/// </param>
public sealed record SaveIntegrationConfigCommand(
    string? Slug,
    bool IsEnabled,
    Dictionary<string, string> Config) : IDemoRestricted;
