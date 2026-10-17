using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>Refuses startup under SSO mode when no backchannel key is configured.</summary>
public sealed class BackchannelLogoutOptionsValidator : IValidateOptions<BackchannelLogoutOptions>
{
    /// <summary>Which sign-in mechanism this deployment runs.</summary>
    private readonly IAuthModeProvider _authMode;

    /// <summary>Initializes a new instance of the <see cref="BackchannelLogoutOptionsValidator"/> class.</summary>
    /// <param name="authMode">Which sign-in mechanism this deployment runs.</param>
    public BackchannelLogoutOptionsValidator(IAuthModeProvider authMode)
    {
        _authMode = authMode;
    }

    /// <summary>Checks the options.</summary>
    /// <param name="name">The options instance name (unused; there is one).</param>
    /// <param name="options">The bound options.</param>
    /// <returns>Success, or a failure naming the missing key.</returns>
    public ValidateOptionsResult Validate(string? name, BackchannelLogoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!_authMode.IsLocalMode && string.IsNullOrWhiteSpace(options.Key))
        {
            return ValidateOptionsResult.Fail(
                $"{BackchannelLogoutOptions.SectionName}:{nameof(BackchannelLogoutOptions.Key)} is required when Auth:Mode is not local "
                + $"(set {BackchannelLogoutOptions.SectionName}__{nameof(BackchannelLogoutOptions.Key)}).");
        }

        return ValidateOptionsResult.Success;
    }
}
