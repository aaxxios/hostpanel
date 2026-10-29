using System.Text;

namespace Innovayse.Hostpanel.Infrastructure.Security;

/// <summary>
/// Recognises a secret that is a template value rather than a choice: the <c>CHANGE_ME</c> markers the
/// shipped <c>.env.example</c> uses, and the literal values earlier templates and the developer
/// fallbacks carried. Such a value is public, so a host running on one is open to anyone who has read
/// the repository.
/// </summary>
public static class KnownPlaceholderSecrets
{
    /// <summary>The marker every current template placeholder starts with.</summary>
    public const string Marker = "CHANGE_ME";

    /// <summary>Values earlier templates shipped and the developer fallbacks, compared without regard to case.</summary>
    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "changeme",
        "postgres",
        "guest",
        "password",
        "secret",
        "change-this-to-a-32-char-min-secret-key-here",
        "dev-hostpanel-backchannel-key",
        "ZGV2LW9ubHkta2V5LW5vdC1mb3ItcHJvZHVjdGlvbiE=",
    };

    /// <summary>Whether a value is a template placeholder.</summary>
    /// <param name="value">The configured secret.</param>
    /// <returns>True when <paramref name="value"/> starts with the marker or equals a known template value; a base64 value is also tested by what it decodes to.</returns>
    public static bool IsPlaceholder(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (IsPlaceholderText(trimmed))
        {
            return true;
        }

        // An encryption key has to be base64 of 32 bytes, so its placeholder is base64 of marker text.
        Span<byte> buffer = new byte[trimmed.Length];
        return Convert.TryFromBase64String(trimmed, buffer, out var written)
            && IsPlaceholderText(Encoding.UTF8.GetString(buffer[..written]));
    }

    /// <summary>Whether plain text starts with the marker or equals a known template value.</summary>
    /// <param name="text">The text to test.</param>
    /// <returns>True for a placeholder.</returns>
    private static bool IsPlaceholderText(string text)
    {
        return text.StartsWith(Marker, StringComparison.OrdinalIgnoreCase) || KnownValues.Contains(text);
    }
}
