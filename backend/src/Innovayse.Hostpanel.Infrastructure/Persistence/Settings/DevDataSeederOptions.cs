using System.ComponentModel.DataAnnotations;

namespace Innovayse.Hostpanel.Infrastructure.Persistence.Settings;

/// <summary>
/// The sign-in passwords <see cref="DevDataSeeder"/> gives the accounts it creates, bound from the
/// <c>DevDataSeed</c> section. They are published development fixtures, not secrets: the class
/// carries no defaults, and only <c>appsettings.Development.json</c> names them, so a deployment
/// that never reads that file (every deployed tier) has no seeded account to sign in to.
/// </summary>
public sealed class DevDataSeederOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "DevDataSeed";

    /// <summary>Password of the seeded administrator account.</summary>
    [Required(AllowEmptyStrings = false)]
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>Password shared by the seeded client accounts.</summary>
    [Required(AllowEmptyStrings = false)]
    public string ClientPassword { get; set; } = string.Empty;
}
