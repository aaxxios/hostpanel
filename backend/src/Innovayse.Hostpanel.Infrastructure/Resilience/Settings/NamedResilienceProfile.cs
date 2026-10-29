namespace Innovayse.Hostpanel.Infrastructure.Resilience.Settings;

/// <summary>A resilience profile paired with the configuration key it binds from.</summary>
/// <param name="Key">The configuration key of the profile.</param>
/// <param name="Profile">The profile.</param>
public sealed record NamedResilienceProfile(string Key, ResilienceProfileOptions Profile);
