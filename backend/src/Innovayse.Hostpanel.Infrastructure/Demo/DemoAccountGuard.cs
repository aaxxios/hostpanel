using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// <see cref="IDemoAccountGuard"/> over <see cref="DemoOptions"/>: reads the switch and the two
/// account emails on every call, so it follows configuration rather than a startup snapshot.
/// </summary>
internal sealed class DemoAccountGuard : IDemoAccountGuard
{
    /// <summary>The demo switch and the published account emails.</summary>
    private readonly IOptions<DemoOptions> _options;

    /// <summary>Resolves an account's current sign-in email from its subject.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Creates the guard.</summary>
    /// <param name="options">The demo switch and the published account emails.</param>
    /// <param name="identity">Resolves an account's current sign-in email from its subject.</param>
    public DemoAccountGuard(IOptions<DemoOptions> options, IIdentityProvider identity)
    {
        _options = options;
        _identity = identity;
    }

    /// <inheritdoc/>
    public async Task<Result> EnsureCredentialsUnchangedAsync(string subject, CancellationToken ct)
    {
        if (!_options.Value.Enabled)
        {
            return Result.Success();
        }

        var account = await _identity.FindBySubjectAsync(subject, ct);
        return account is null ? Result.Success() : EnsureCredentialsUnchanged(account.Email);
    }

    /// <inheritdoc/>
    public Result EnsureCredentialsUnchanged(string email)
    {
        var options = _options.Value;
        if (!options.Enabled)
        {
            return Result.Success();
        }

        var isDemoAccount = Matches(email, options.AdminEmail) || Matches(email, options.ClientEmail);
        return isDemoAccount ? Result.FailureCode(HostpanelErrorCodes.DEMO_ACTION_DISABLED) : Result.Success();
    }

    /// <summary>Whether an email equals a configured demo email, ignoring case and surrounding blanks.</summary>
    /// <param name="email">The email under test.</param>
    /// <param name="demoEmail">The configured demo email, possibly unset.</param>
    /// <returns><see langword="true"/> when they name the same address.</returns>
    private static bool Matches(string? email, string? demoEmail) =>
        !string.IsNullOrWhiteSpace(demoEmail)
        && string.Equals(email?.Trim(), demoEmail.Trim(), StringComparison.OrdinalIgnoreCase);
}
