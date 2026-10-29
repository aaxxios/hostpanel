using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Local-mode two-factor authentication: the account, secret and codes all live in this
/// product's own database, reached through the existing <see cref="IUserService"/>.
///
/// <para>
/// A thin delegate rather than a place that owns the TOTP logic itself, so as not to touch
/// <see cref="IUserService"/>'s other 15+ unrelated members or its one implementation for a
/// fix scoped to three handlers. The Issuer label and enrolment URI shape live here because
/// building that URI is TOTP-specific, not something
/// <see cref="Innovayse.Hostpanel.Application.Auth.Commands.StartTwoFactorSetup.StartTwoFactorSetupHandler"/>
/// needs to know about when it only calls this interface.
/// </para>
/// </summary>
public sealed class LocalTwoFactorService : ITwoFactorService
{
    /// <summary>Identity access: the local implementation this delegates every call to.</summary>
    private readonly IUserService _users;

    /// <summary>The label an authenticator app shows. Local mode is what a self-hosted, third-party deployment uses, one with no external sign-on provider of its own, so the name burned into every enrolled account's QR code must be that deployment's own, not this product's.</summary>
    private readonly IOptions<LocalTwoFactorOptions> _options;

    /// <summary>Initializes a new instance of the <see cref="LocalTwoFactorService"/> class.</summary>
    /// <param name="users">Identity access: the local implementation this delegates every call to.</param>
    /// <param name="options">The label an authenticator app shows. Local mode is what a self-hosted, third-party deployment uses, one with no external sign-on provider of its own, so the name burned into every enrolled account's QR code must be that deployment's own, not this product's.</param>
    public LocalTwoFactorService(IUserService users, IOptions<LocalTwoFactorOptions> options)
    {
        _users = users;
        _options = options;
    }

    /// <summary>The name shown in the authenticator app beside the account.</summary>
    private string Issuer => _options.Value.Issuer;

    /// <inheritdoc/>
    public async Task<Result<TwoFactorSetupDto>> StartSetupAsync(string userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId, ct);
        if (user is null)
        {
            return Result<TwoFactorSetupDto>.FailureCode(SharedErrorCodes.Unauthorized);
        }

        // Refused rather than re-issued: a second setup on an enabled account is what let a
        // signed-in session swap the secret and take the second factor over.
        if (await _users.IsTwoFactorEnabledAsync(userId, ct))
        {
            return Result<TwoFactorSetupDto>.FailureCode(HostpanelErrorCodes.TWO_FACTOR_ALREADY_ENABLED);
        }

        var secret = await _users.GenerateTwoFactorSecretAsync(userId, ct);

        // Both label halves are escaped: an address with a colon or a space in it would
        // otherwise split the label and produce a URI the app reads as a different account.
        var label = $"{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(user.Email)}";
        var uri = $"otpauth://totp/{label}?secret={secret}&issuer={Uri.EscapeDataString(Issuer)}";

        return Result<TwoFactorSetupDto>.Success(new TwoFactorSetupDto(secret, uri));
    }

    /// <inheritdoc/>
    public async Task<Result> EnableAsync(TwoFactorConfirmation confirmation, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(confirmation);

        var enabled = await _users.EnableTwoFactorAsync(confirmation.UserId, confirmation.Code, ct);
        return enabled ? Result.Success() : Result.FailureCode(HostpanelErrorCodes.TWO_FACTOR_CODE_INVALID);
    }

    /// <inheritdoc/>
    public async Task<Result> DisableAsync(TwoFactorRemoval removal, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(removal);

        // The password first: a code alone proves the device, the password proves the person,
        // and a session someone else is holding has neither.
        if (string.IsNullOrEmpty(removal.CurrentPassword)
            || !await _users.CheckPasswordAsync(removal.UserId, removal.CurrentPassword, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.CURRENT_PASSWORD_INVALID);
        }

        if (!await _users.VerifyTwoFactorCodeAsync(removal.UserId, removal.Code, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.TWO_FACTOR_CODE_INVALID);
        }

        await _users.DisableTwoFactorAsync(removal.UserId, ct);
        return Result.Success();
    }
}
