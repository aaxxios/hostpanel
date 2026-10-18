using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Innovayse.Hostpanel.Infrastructure.Auth;
/// <summary>
/// Creates and edits people in this product's own <c>AspNetUsers</c> table.
///
/// <para>
/// What a deployment with no SSO gets, and unchanged behaviour: the same Identity calls
/// this product has always made, behind a name the SSO-owned deployment can refuse.
/// </para>
/// </summary>
public sealed class LocalUserProvisioning : IUserProvisioning
{
    /// <summary>The users this class uses.</summary>
    private readonly UserManager<AppUser> _users;

    /// <summary>Initializes a new instance of the <see cref="LocalUserProvisioning"/> class.</summary>
    /// <param name="users">The users this class uses.</param>
    public LocalUserProvisioning(UserManager<AppUser> users)
    {
        _users = users;
    }

    /// <inheritdoc/>
    public async Task<Result<string>> CreateAsync(
        string email, string password, string? firstName, string? lastName, CancellationToken ct)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            // Falling back to the local part of the address rather than to an empty
            // string: a person with no name at all shows as a blank row everywhere.
            FirstName = string.IsNullOrWhiteSpace(firstName) ? email.Split('@')[0] : firstName,
            LastName = lastName ?? string.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await _users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not create {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return Result<string>.Success(user.Id);
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateProfileAsync(
        string subject, string firstName, string lastName, string? language, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return Result.Success();
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Language = language;

        await ThrowIfFailedAsync(_users.UpdateAsync(user), subject);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> ChangeEmailAsync(string subject, string email, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return Result.Success();
        }

        // The user name is the address here — Identity was configured that way, and
        // leaving it behind would let somebody sign in under the old address forever.
        user.Email = email;
        user.UserName = email;

        await ThrowIfFailedAsync(_users.UpdateAsync(user), subject);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(string subject, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null || user.IsDeleted)
        {
            return Result.Success();
        }

        // Marked, not removed. See the note on AppUser.DeletedAt: the client and invoice
        // rows that reference this id outlive the account, and a removed row turns every
        // one of them into a blank nobody can identify.
        //
        // The address stays on the row, so it stays taken — which is what the SSO does
        // with its own deleted accounts, and it keeps the two stores answering alike. It
        // also means signing the same person back up needs the old account restored rather
        // than silently shadowed by a second one holding the same address.
        user.DeletedAt = DateTimeOffset.UtcNow;

        // Locked out at the same time, so any path that reads the row while ignoring the
        // deleted marker still refuses the sign-in rather than letting it through.
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        await ThrowIfFailedAsync(_users.UpdateAsync(user), subject);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> SetPasswordAsync(string subject, string password, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return Result<bool>.Success(false);
        }

        // Through a reset token rather than by removing and re-adding the password. The
        // remove-then-add pair leaves the account with no password at all if the second
        // call fails, and this one is atomic.
        var token = await _users.GeneratePasswordResetTokenAsync(user);
        await ThrowIfFailedAsync(_users.ResetPasswordAsync(user, token, password), subject);
        return Result<bool>.Success(true);
    }

    /// <inheritdoc/>
    public async Task<Result<string?>> IssuePasswordResetTokenAsync(string subject, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return Result<string?>.Success(null);
        }

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        return Result<string?>.Success(token);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ResetPasswordWithTokenAsync(
        string email, string token, string newPassword, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<bool>.Success(false);
        }

        var result = await _users.ResetPasswordAsync(user, token, newPassword);
        return Result<bool>.Success(result.Succeeded);
    }

    /// <inheritdoc/>
    public async Task<Result> ConfirmEmailAsync(string subject, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return Result.Success();
        }

        user.EmailConfirmed = true;

        await ThrowIfFailedAsync(_users.UpdateAsync(user), subject);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<string?>> IssueEmailConfirmationTokenAsync(string subject, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(subject);
        if (user is null)
        {
            return Result<string?>.Success(null);
        }

        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        return Result<string?>.Success(token);
    }

    /// <summary>
    /// Identity reports a rejected write by returning it, not by throwing. A duplicate
    /// address or a failed validation would otherwise be indistinguishable from success —
    /// the caller commits its own changes and the account keeps the details it had.
    /// </summary>
    /// <param name="update">The pending Identity write.</param>
    /// <param name="subject">Whose account the write was for, for the exception message.</param>
    /// <exception cref="InvalidOperationException">
    /// The write is a genuine infrastructure failure (a duplicate address, a rejected
    /// validation), not a "not allowed here" refusal, so it is not a <see cref="Result"/>: no
    /// caller of this port is prepared to act on it, and it belongs in the log rather than in a
    /// code a frontend would branch on.
    /// </exception>
    private static async Task ThrowIfFailedAsync(Task<IdentityResult> update, string subject)
    {
        var result = await update;
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Could not update {subject}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
