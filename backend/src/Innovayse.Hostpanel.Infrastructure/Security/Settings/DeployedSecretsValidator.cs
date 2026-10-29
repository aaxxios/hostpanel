using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Security.Settings;

/// <summary>
/// Refuses startup on a deployed tier when a secret is a template placeholder or too short to resist
/// guessing. A host that boots on <c>CHANGE_ME</c> or on a value published in the repository looks
/// configured and is open, so the failure names every offending key and the variable that sets it.
/// Development and the test host are exempt: a throwaway value is the point there, and the template's
/// own values keep a laptop working.
/// </summary>
public sealed class DeployedSecretsValidator : IValidateOptions<DeployedSecrets>
{
    /// <summary>The fewest characters a token signing secret may have (the signer needs 32 for HMAC-SHA256).</summary>
    public const int MinimumJwtSecretLength = 32;

    /// <summary>The fewest characters a sign-out key may have when one is configured.</summary>
    public const int MinimumBackchannelKeyLength = 16;

    /// <summary>The fewest characters a database password may have when the connection uses one.</summary>
    public const int MinimumDatabasePasswordLength = 12;

    /// <summary>The tier this process runs as.</summary>
    private readonly IHostEnvironment _environment;

    /// <summary>Initializes a new instance of the <see cref="DeployedSecretsValidator"/> class.</summary>
    /// <param name="environment">The tier this process runs as.</param>
    public DeployedSecretsValidator(IHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>Checks the options.</summary>
    /// <param name="name">The options instance name (unused; there is one).</param>
    /// <param name="options">The gathered secrets.</param>
    /// <returns>Success, or a failure naming every refused secret.</returns>
    public ValidateOptionsResult Validate(string? name, DeployedSecrets options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (_environment.IsDevelopment() || _environment.IsEnvironment(EnvironmentNames.Testing))
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        Check(failures, "Jwt:Secret (Jwt__Secret, JWT_SECRET)", options.JwtSecret, MinimumJwtSecretLength, required: true);
        Check(failures, "EncryptionKey (ENCRYPTION_KEY)", options.EncryptionKey, minimumLength: 0, required: true);
        Check(failures, "BackchannelLogout:Key (BackchannelLogout__Key, HOSTPANEL_BACKCHANNEL_LOGOUT_KEY)", options.BackchannelLogoutKey, MinimumBackchannelKeyLength, required: false);
        Check(failures, "the database password (POSTGRES_PASSWORD)", options.DatabasePassword, MinimumDatabasePasswordLength, required: false);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    /// <summary>Records a failure when a secret is missing, a placeholder or too short.</summary>
    /// <param name="failures">Receives the failure sentence.</param>
    /// <param name="label">The secret and the variables that set it, for the sentence.</param>
    /// <param name="value">The configured value.</param>
    /// <param name="minimumLength">The fewest characters allowed; zero skips the length check.</param>
    /// <param name="required">Whether an empty value is itself a failure; an optional secret is only judged when set.</param>
    private static void Check(List<string> failures, string label, string? value, int minimumLength, bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                failures.Add($"{label} is not set. Generate a value of your own (for example with `openssl rand -hex 32`).");
            }

            return;
        }

        if (KnownPlaceholderSecrets.IsPlaceholder(value))
        {
            failures.Add($"{label} is still a template placeholder or a published example value. Generate a value of your own (for example with `openssl rand -hex 32`).");
            return;
        }

        if (value.Length < minimumLength)
        {
            failures.Add($"{label} is shorter than {minimumLength} characters. Generate a longer value (for example with `openssl rand -hex 32`).");
        }
    }
}
