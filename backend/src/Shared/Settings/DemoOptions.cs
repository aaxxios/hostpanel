namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>
/// The public demo deployment's switch and its published accounts. Every value arrives from the
/// environment (<c>Demo__*</c>); the class and <c>appsettings.json</c> carry no defaults, so an
/// operator who turns the demo on has to say every value out loud, and
/// <see cref="DemoOptionsValidator"/> refuses startup when one is missing.
/// </summary>
public sealed class DemoOptions
{
    /// <summary>The configuration section these options bind from.</summary>
    public const string SectionName = "Demo";

    /// <summary>Whether the deployment is a demo: fake outward adapters, the guard, seeded data and the periodic reset.</summary>
    public bool Enabled { get; init; }

    /// <summary>Hours between two resets of the business data; must be positive when <see cref="Enabled"/>.</summary>
    public int ResetIntervalHours { get; init; }

    /// <summary>Sign-in email of the published demo administrator.</summary>
    public string? AdminEmail { get; init; }

    /// <summary>Password of the published demo administrator.</summary>
    public string? AdminPassword { get; init; }

    /// <summary>Sign-in email of the published demo client.</summary>
    public string? ClientEmail { get; init; }

    /// <summary>Password of the published demo client.</summary>
    public string? ClientPassword { get; init; }
}
