using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Auth.Settings;

/// <summary>Refuses startup when the session lifetimes are not usable.</summary>
public sealed class SessionOptionsValidator : IValidateOptions<SessionOptions>
{
    /// <summary>Checks the options.</summary>
    /// <param name="name">The options instance name (unused; there is one).</param>
    /// <param name="options">The bound options.</param>
    /// <returns>Success, or a failure naming the offending key.</returns>
    public ValidateOptionsResult Validate(string? name, SessionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.AccessTokenMinutes <= 0)
        {
            return ValidateOptionsResult.Fail(
                $"{SessionOptions.SectionName}:{nameof(SessionOptions.AccessTokenMinutes)} must be greater than zero "
                + $"(set {SessionOptions.SectionName}__{nameof(SessionOptions.AccessTokenMinutes)}).");
        }

        if (options.MaxSessionMinutes < options.AccessTokenMinutes)
        {
            return ValidateOptionsResult.Fail(
                $"{SessionOptions.SectionName}:{nameof(SessionOptions.MaxSessionMinutes)} must not be shorter than "
                + $"{SessionOptions.SectionName}:{nameof(SessionOptions.AccessTokenMinutes)} "
                + $"(set {SessionOptions.SectionName}__{nameof(SessionOptions.MaxSessionMinutes)}).");
        }

        return ValidateOptionsResult.Success;
    }
}
