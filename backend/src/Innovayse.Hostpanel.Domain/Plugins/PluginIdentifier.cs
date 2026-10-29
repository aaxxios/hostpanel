using System.Text.RegularExpressions;

namespace Innovayse.Hostpanel.Domain.Plugins;

/// <summary>
/// The rule a plugin id must meet. A plugin id names the plugin's own folder, a settings key prefix
/// and a URL segment, so only lower-case letters, digits and single hyphens between them are
/// accepted: nothing that a file system or a URL could read as a separator, a parent folder or a
/// drive.
/// </summary>
public static partial class PluginIdentifier
{
    /// <summary>The longest id accepted.</summary>
    public const int MaxLength = 64;

    /// <summary>The accepted shape: lower-case alphanumeric words joined by single hyphens.</summary>
    public const string Pattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";

    /// <summary>Whether <paramref name="id"/> is an acceptable plugin id.</summary>
    /// <param name="id">The candidate id, as read from a manifest, a folder name or a route.</param>
    /// <returns><see langword="true"/> when the id is non-empty, short enough and matches <see cref="Pattern"/>.</returns>
    public static bool IsValid(string? id)
    {
        return !string.IsNullOrEmpty(id) && id.Length <= MaxLength && PatternRegex().IsMatch(id);
    }

    /// <summary>The compiled <see cref="Pattern"/>.</summary>
    /// <returns>The regular expression.</returns>
    [GeneratedRegex(Pattern, RegexOptions.CultureInvariant)]
    private static partial Regex PatternRegex();
}
