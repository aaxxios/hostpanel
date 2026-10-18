using System.Security.Cryptography;
using System.Text;
using Innovayse.Hostpanel.Application.Auth.Seeding;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Auth.Commands.CompleteSetup;

/// <summary>
/// Handles <see cref="CompleteSetupCommand"/> — the first-run bootstrap that grants the Admin
/// role to the caller, once, and only to a caller who can prove they are the operator.
/// </summary>
public sealed class CompleteSetupHandler
{
    /// <summary>Role store: asked whether Admin is unclaimed, then written to.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>The module's data context, holding the outstanding setup token.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Which sign-in mechanism this deployment runs.</summary>
    private readonly IAuthModeProvider _authMode;

    /// <summary>Unit of work for persisting the retired token.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Who is claiming; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Writes the claimant's account; used to confirm their email in local mode.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="roles">Role store: asked whether Admin is unclaimed, then written to.</param>
    /// <param name="context">The module's data context, holding the outstanding setup token.</param>
    /// <param name="authMode">Which sign-in mechanism this deployment runs.</param>
    /// <param name="uow">Unit of work for persisting the retired token.</param>
    /// <param name="caller">Who is claiming; the command does not say, and must not.</param>
    /// <param name="provisioning">Writes the claimant's account; used to confirm their email in local mode.</param>
    public CompleteSetupHandler(
        ISubjectRoleStore roles,
        IHostpanelDbContext context,
        IAuthModeProvider authMode,
        IUnitOfWork uow,
        ICurrentUserService caller,
        IUserProvisioning provisioning)
    {
        _roles = roles;
        _context = context;
        _authMode = authMode;
        _uow = uow;
        _caller = caller;
        _provisioning = provisioning;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The command, carrying the setup token and nothing else.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the role has been granted; <c>SETUP_ALREADY_COMPLETED</c> when somebody
    /// already holds Admin; <c>SETUP_TOKEN_INVALID</c> under local mode when the token is
    /// missing, wrong, or this installation has none outstanding; <c>UNAUTHORIZED</c> when the
    /// request carries no subject.
    /// </returns>
    public async Task<Result> HandleAsync(CompleteSetupCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Asked before the token is even looked at, so a caller with the wrong token cannot use
        // the difference between the two refusals to learn whether the installation is still
        // claimable. The window this closes is small, but it is free to close.
        if (await _roles.AnyHasRoleAsync(Roles.Admin, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.SETUP_ALREADY_COMPLETED);
        }

        var subject = _caller.UserId;
        if (subject is null)
        {
            return Result.FailureCode(SharedErrorCodes.Unauthorized);
        }

        // Under sso the gate is inert: no token was ever issued, none is asked for, and this
        // path behaves exactly as it did before the gate existed. Accounts there belong to the
        // sign-on service, so the callers who can reach an authenticated endpoint at all are
        // already the ones the operator provisioned — and that path is in production use.
        if (_authMode.IsLocalMode)
        {
            var tokenResult = await ConsumeSetupTokenAsync(cmd.SetupToken, ct);
            if (tokenResult.IsFailure)
            {
                return tokenResult;
            }

            // Consuming the token proves control of the install, and no verification mail is
            // ever sent for this account, so confirm the address here or the first admin is
            // stuck on the "Verify your email" screen. Sso accounts are confirmed by the SSO.
            var confirmed = await _provisioning.ConfirmEmailAsync(subject, ct);
            if (confirmed.IsFailure)
            {
                return confirmed;
            }
        }

        await _roles.AddAsync(subject, Roles.Admin, ct);
        return Result.Success();
    }

    /// <summary>
    /// Verifies the presented setup token against the outstanding one and blanks it.
    /// </summary>
    /// <param name="presented">The token the caller sent, if any.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success once the token has been retired, or <c>SETUP_TOKEN_INVALID</c>.</returns>
    private async Task<Result> ConsumeSetupTokenAsync(string? presented, CancellationToken ct)
    {
        // Was ISettingRepository.FindByKeyAsync. Tracked, not AsNoTracking: the row is blanked
        // and saved further down.
        var issued = await _context.Settings.FirstOrDefaultAsync(s => s.Key == SetupTokenSeeder.SettingKey, ct);

        // Fails closed. A missing or blank row means either that this installation has already
        // been bootstrapped through the gate, or that the boot-time issuer never ran — and in
        // neither case is granting Admin to whoever asked the safe reading. The sentence tells
        // the operator to restart the API, which re-issues and re-logs a token.
        if (issued is null || string.IsNullOrWhiteSpace(issued.Value) || string.IsNullOrEmpty(presented))
        {
            return Result.FailureCode(HostpanelErrorCodes.SETUP_TOKEN_INVALID);
        }

        // Fixed-time comparison. The tokens are 256-bit random, so a timing oracle is not the
        // realistic attack here, but a byte-by-byte early exit on the one secret that guards
        // ownership of the installation is not worth defending in review.
        var matches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(issued.Value),
            Encoding.UTF8.GetBytes(presented));

        if (!matches)
        {
            return Result.FailureCode(HostpanelErrorCodes.SETUP_TOKEN_INVALID);
        }

        // Retired by blanking rather than deleting: ISettingRepository exposes no removal, an
        // empty value is refused above exactly as a missing row is, and the row left behind
        // records that this installation was bootstrapped through the gate. Saved here rather
        // than after the grant so that a failure between the two leaves the token spent rather
        // than the role granted and the token still live.
        issued.UpdateValue(string.Empty);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
