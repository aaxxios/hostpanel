using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Answers from the <c>Auth:Mode</c> value the composition root read once at startup.
///
/// <para>
/// Registered as a singleton: configuration does not change while the process runs, and
/// every caller asking the same question should get the same answer computed the same
/// way, rather than each holding its own copy of the comparison. The comparison itself is
/// case-insensitive, and made once: call sites comparing the value each in their own way can
/// disagree about which mode the process is in when it is configured as <c>Local</c> or
/// <c>SSO</c>.
/// </para>
/// </summary>
public sealed class ConfigurationAuthModeProvider : IAuthModeProvider
{
    /// <summary>Initializes a new instance of the <see cref="ConfigurationAuthModeProvider"/> class.</summary>
    /// <param name="isLocalMode">Whether this deployment owns its users, computed by <see cref="AuthMode.IsLocal"/> from <c>Auth:Mode</c> in <c>DependencyInjection</c>; no class below the root reads configuration.</param>
    public ConfigurationAuthModeProvider(bool isLocalMode)
    {
        IsLocalMode = isLocalMode;
    }

    /// <inheritdoc/>
    public bool IsLocalMode { get; }
}
