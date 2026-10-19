using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Shared.Settings;

/// <summary>
/// Refuses startup when the demo is switched on with a value missing. A disabled demo is always
/// valid: nothing reads the other values then.
/// </summary>
public sealed class DemoOptionsValidator : IValidateOptions<DemoOptions>
{
    /// <summary>
    /// The shortest password the identity store accepts. Mirrors <c>opts.Password.RequiredLength</c>
    /// in <c>Infrastructure/DependencyInjection.cs</c> (<c>AddIdentityCore</c>); Shared cannot
    /// reference Infrastructure, so the rules are copied and must change together.
    /// </summary>
    private const int MinimumPasswordLength = 8;

    /// <summary>Checks the options.</summary>
    /// <param name="name">The options instance name (unused; there is one).</param>
    /// <param name="options">The bound options.</param>
    /// <returns>Success, or a failure naming every missing or malformed key.</returns>
    public ValidateOptionsResult Validate(string? name, DemoOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        if (options.ResetIntervalHours <= 0)
        {
            failures.Add($"{DemoOptions.SectionName}:{nameof(DemoOptions.ResetIntervalHours)} must be a positive number of hours (set Demo__ResetIntervalHours).");
        }

        CheckEmail(failures, nameof(DemoOptions.AdminEmail), options.AdminEmail);
        CheckEmail(failures, nameof(DemoOptions.ClientEmail), options.ClientEmail);
        CheckPassword(failures, nameof(DemoOptions.AdminPassword), options.AdminPassword);
        CheckPassword(failures, nameof(DemoOptions.ClientPassword), options.ClientPassword);

        // Optional, so only a value that is set is checked: empty keeps the seed's fictional address.
        if (!string.IsNullOrWhiteSpace(options.ContactEmail) && !options.ContactEmail.Contains('@', StringComparison.Ordinal))
        {
            failures.Add($"{DemoOptions.SectionName}:{nameof(DemoOptions.ContactEmail)} must be an email address when set (set Demo__{nameof(DemoOptions.ContactEmail)} or leave it empty).");
        }

        if (!string.IsNullOrWhiteSpace(options.AdminEmail)
            && string.Equals(options.AdminEmail, options.ClientEmail, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"{DemoOptions.SectionName}:{nameof(DemoOptions.AdminEmail)} and {nameof(DemoOptions.ClientEmail)} must differ.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    /// <summary>Records a failure when an email is absent or not an address.</summary>
    /// <param name="failures">Receives the failure sentence.</param>
    /// <param name="key">The option's name, for the sentence.</param>
    /// <param name="value">The configured value.</param>
    private static void CheckEmail(List<string> failures, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@', StringComparison.Ordinal))
        {
            failures.Add($"{DemoOptions.SectionName}:{key} is required when the demo is enabled and must be an email address (set Demo__{key}).");
        }
    }

    /// <summary>
    /// Records a failure when a password is absent or breaks a rule Identity enforces: at least
    /// <see cref="MinimumPasswordLength"/> characters, one upper-case and one lower-case letter
    /// (digits and symbols are not required there, so they are not required here).
    /// </summary>
    /// <param name="failures">Receives the failure sentence.</param>
    /// <param name="key">The option's name, for the sentence.</param>
    /// <param name="value">The configured value.</param>
    private static void CheckPassword(List<string> failures, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length < MinimumPasswordLength
            || !value.Any(char.IsUpper)
            || !value.Any(char.IsLower))
        {
            failures.Add($"{DemoOptions.SectionName}:{key} is required when the demo is enabled and must be at least {MinimumPasswordLength} characters with an upper-case and a lower-case letter (set Demo__{key}).");
        }
    }
}
