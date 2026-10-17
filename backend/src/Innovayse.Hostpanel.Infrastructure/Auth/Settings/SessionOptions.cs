namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>
/// Lifetimes of a local-mode session, bound from the <c>Session</c> section.
/// </summary>
/// <remarks>
/// An access token is short so a leaked one dies on its own; the API re-issues it while the
/// person is still working (sliding renewal), but never past <see cref="MaxSessionMinutes"/>
/// counted from the sign-in, so a session cannot be kept alive forever.
/// </remarks>
public sealed class SessionOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Session";

    /// <summary>How long one issued access token stays valid, in minutes.</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>The absolute age, in minutes from sign-in, after which a session is never renewed again.</summary>
    public int MaxSessionMinutes { get; set; } = 720;
}
