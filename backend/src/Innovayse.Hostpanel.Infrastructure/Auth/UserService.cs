using System.Security.Cryptography;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Auth.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using OtpNet;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// Implements <see cref="IUserService"/> using ASP.NET Core Identity's <see cref="UserManager{TUser}"/>.
/// </summary>
public sealed class UserService : IUserService
{
    /// <summary>The stand-in account an unknown address is checked against, so it costs a real hash check.</summary>
    private static readonly AppUser UnknownAccount = new() { FirstName = string.Empty, LastName = string.Empty };

    /// <summary>
    /// The hash an unknown address is checked against, made once by the configured hasher on first
    /// use. Two requests racing to make it both produce a valid hash, so no lock is needed.
    /// </summary>
    private static string? _unknownAccountHash;

    /// <summary>The Identity user manager for <see cref="AppUser"/>.</summary>
    private readonly UserManager<AppUser> _userManager;

    /// <summary>The per-account cap on wrong codes and the replay guard for accepted ones.</summary>
    private readonly ITwoFactorCodeGuard _codeGuard;

    /// <summary>The clock a one-time code and a sign-in time are read from.</summary>
    private readonly TimeProvider _time;

    /// <summary>Tells the published demo accounts apart, which wrong passwords never lock.</summary>
    private readonly IDemoAccountGuard _demoAccounts;

    /// <summary>Initializes a new instance of the <see cref="UserService"/> class.</summary>
    /// <param name="userManager">The Identity user manager for <see cref="AppUser"/>.</param>
    /// <param name="codeGuard">The per-account cap on wrong codes and the replay guard for accepted ones.</param>
    /// <param name="time">The clock a one-time code and a sign-in time are read from.</param>
    /// <param name="demoAccounts">Tells the published demo accounts apart, which wrong passwords never lock.</param>
    public UserService(
        UserManager<AppUser> userManager, ITwoFactorCodeGuard codeGuard, TimeProvider time, IDemoAccountGuard demoAccounts)
    {
        _userManager = userManager;
        _codeGuard = codeGuard;
        _time = time;
        _demoAccounts = demoAccounts;
    }

    /// <inheritdoc/>
    public async Task<string> CreateAsync(CreateUserInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);

        var user = new AppUser
        {
            UserName = input.Email,
            Email = input.Email,
            FirstName = input.FirstName ?? string.Empty,
            LastName = input.LastName ?? string.Empty,
        };

        var result = await _userManager.CreateAsync(user, input.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        return user.Id;
    }

    /// <inheritdoc/>
    public async Task AddToRoleAsync(string userId, string role, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        await _userManager.AddToRoleAsync(user, role);
    }

    /// <inheritdoc/>
    public async Task<PasswordSignInResult> SignInWithPasswordAsync(PasswordSignInInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);

        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user is null || user.IsDeleted)
        {
            // The same hash check a real account costs, so the answer time does not tell which
            // addresses exist. The result is meaningless and ignored.
            _userManager.PasswordHasher.VerifyHashedPassword(UnknownAccount, UnknownAccountHash(), input.Password);
            return new PasswordSignInResult(PasswordSignInOutcome.Failed, null);
        }

        // A locked account is refused before its password is looked at, so guessing on cannot
        // learn anything until the lockout ends.
        if (await _userManager.IsLockedOutAsync(user))
        {
            return new PasswordSignInResult(PasswordSignInOutcome.LockedOut, null);
        }

        if (!await _userManager.CheckPasswordAsync(user, input.Password))
        {
            // The demo accounts are published for everyone: counting wrong passwords against them
            // would let any visitor lock the demo for all the others.
            if (_demoAccounts.IsPublishedDemoAccount(user.Email!))
            {
                return new PasswordSignInResult(PasswordSignInOutcome.Failed, null);
            }

            await _userManager.AccessFailedAsync(user);
            var outcome = await _userManager.IsLockedOutAsync(user)
                ? PasswordSignInOutcome.LockedOut
                : PasswordSignInOutcome.Failed;
            return new PasswordSignInResult(outcome, null);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return new PasswordSignInResult(PasswordSignInOutcome.Succeeded, new UserIdentity(user.Id, user.Email!));
    }

    /// <inheritdoc/>
    public async Task<UserIdentity?> FindByIdAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : new UserIdentity(user.Id, user.Email!);
    }

    /// <inheritdoc/>
    public async Task<bool> IsEmailConfirmedAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.EmailConfirmed ?? false;
    }

    /// <inheritdoc/>
    public async Task UpdateLastLoginAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        user.LastLoginAt = _time.GetUtcNow();
        await _userManager.UpdateAsync(user);
    }

    /// <inheritdoc/>
    public async Task<bool> IsTwoFactorEnabledAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.TwoFactorEnabled ?? false;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateTwoFactorSecretAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        var secretBytes = KeyGeneration.GenerateRandomKey(20);
        var secret = Base32Encoding.ToString(secretBytes);

        // Pending, never active: an enabled account keeps signing in with the secret it has until
        // a code proves the authenticator app holds this one.
        user.PendingTwoFactorSecret = secret;
        await _userManager.UpdateAsync(user);
        return secret;
    }

    /// <inheritdoc/>
    public async Task<bool> EnableTwoFactorAsync(string userId, string code, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !VerifyAgainst(userId, user.PendingTwoFactorSecret, code))
        {
            return false;
        }

        user.TwoFactorSecret = user.PendingTwoFactorSecret;
        user.PendingTwoFactorSecret = null;

        // Set directly rather than through SetTwoFactorEnabledAsync, which would also change the
        // security stamp: switching the second factor on strengthens the account, and signing the
        // person out of the session they just used to do it would only be in their way.
        user.TwoFactorEnabled = true;
        await _userManager.UpdateAsync(user);
        return true;
    }

    /// <inheritdoc/>
    public async Task DisableTwoFactorAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        user.TwoFactorSecret = null;
        user.PendingTwoFactorSecret = null;

        // Identity changes the security stamp as part of this call, which is what ends every
        // session issued before the second factor was removed.
        await _userManager.SetTwoFactorEnabledAsync(user, false);
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyTwoFactorCodeAsync(string userId, string code, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return VerifyAgainst(userId, user?.TwoFactorSecret, code);
    }

    /// <inheritdoc/>
    public async Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null && !user.IsDeleted && await _userManager.CheckPasswordAsync(user, password);
    }

    /// <inheritdoc/>
    public async Task<string?> GetSessionVersionAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
        {
            return null;
        }

        // An account written outside Identity (an import, an old seed) can have no stamp yet;
        // it gets one here, once, rather than every token for it failing to be issued.
        if (string.IsNullOrEmpty(user.SecurityStamp))
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }

        return SessionVersion.FromSecurityStamp(user.SecurityStamp!);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return [];
        }

        return [.. await _userManager.GetRolesAsync(user)];
    }

    /// <summary>
    /// Checks a code against one secret under the account's attempt cap and replay guard.
    /// </summary>
    /// <param name="userId">Whose attempts the guard counts.</param>
    /// <param name="secret">The Base32 secret to check against, or null when the account has none.</param>
    /// <param name="code">The 6-digit TOTP code from the authenticator app.</param>
    /// <returns>True when the code is valid within the allowed time window and was not used before.</returns>
    private bool VerifyAgainst(string userId, string? secret, string code)
    {
        // Every door that checks a code (sign-in, enabling, disabling) comes through here, so the
        // limits live here. The attempt is taken before the code is looked at, so parallel guesses
        // cannot overshoot the cap; a correct code gives it back.
        if (!_codeGuard.TryBeginAttempt(userId))
        {
            return false;
        }

        if (secret is null)
        {
            return false;
        }

        var totp = new Totp(Base32Encoding.ToBytes(secret));
        if (!totp.VerifyTotp(_time.GetUtcNow().UtcDateTime, code, out var timeStep, VerificationWindow.RfcSpecifiedNetworkDelay))
        {
            return false;
        }

        // A code that was right once is not right again: it is a replay, and counts against the cap.
        if (!_codeGuard.TryAcceptTimeStep(userId, timeStep))
        {
            return false;
        }

        _codeGuard.Forgive(userId);
        return true;
    }

    /// <summary>Returns the hash an unknown address is checked against, making it on first use.</summary>
    /// <returns>A hash of a random password, made by the configured hasher.</returns>
    private string UnknownAccountHash()
    {
        return _unknownAccountHash ??= _userManager.PasswordHasher.HashPassword(
            UnknownAccount, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    }
}
