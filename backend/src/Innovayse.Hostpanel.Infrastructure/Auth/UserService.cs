using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace Innovayse.Hostpanel.Infrastructure.Auth;
/// <summary>
/// Implements <see cref="IUserService"/> using ASP.NET Core Identity's <see cref="UserManager{TUser}"/>.
/// </summary>
public sealed class UserService : IUserService
{
    /// <summary>The Identity user manager for <see cref="AppUser"/>.</summary>
    private readonly UserManager<AppUser> _userManager;

    /// <summary>The application DbContext, for the linked client account.</summary>
    private readonly AppDbContext _db;

    /// <summary>Unit of work for persisting changes.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>The per-account cap on wrong codes and the replay guard for accepted ones.</summary>
    private readonly ITwoFactorCodeGuard _codeGuard;

    /// <summary>The clock a one-time code is checked against.</summary>
    private readonly TimeProvider _time;

    /// <summary>Initializes a new instance of the <see cref="UserService"/> class.</summary>
    /// <param name="userManager">The Identity user manager for <see cref="AppUser"/>.</param>
    /// <param name="db">The application DbContext, for the linked client account.</param>
    /// <param name="uow">Unit of work for persisting changes.</param>
    /// <param name="codeGuard">The per-account cap on wrong codes and the replay guard for accepted ones.</param>
    /// <param name="time">The clock a one-time code is checked against.</param>
    public UserService(UserManager<AppUser> userManager, AppDbContext db, IUnitOfWork uow, ITwoFactorCodeGuard codeGuard, TimeProvider time)
    {
        _userManager = userManager;
        _db = db;
        _uow = uow;
        _codeGuard = codeGuard;
        _time = time;
    }

    /// <inheritdoc/>
    public async Task<string> CreateAsync(string email, string password, string? firstName = null, string? lastName = null, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName ?? string.Empty,
            LastName = lastName ?? string.Empty,
        };

        var result = await _userManager.CreateAsync(user, password);
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
    public async Task<(string Id, string Email)?> FindByEmailAndPasswordAsync(
        string email,
        string password,
        CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var valid = await _userManager.CheckPasswordAsync(user, password);
        return valid ? (user.Id, user.Email!) : null;
    }

    /// <inheritdoc/>
    public async Task<(string Id, string Email)?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : (user.Id, user.Email!);
    }

    /// <inheritdoc/>
    public async Task<(string Id, string Email)?> FindByIdAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : (user.Id, user.Email!);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, string>> GetEmailsByIdsAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        var result = new Dictionary<string, string>();
        foreach (var id in userIds)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user?.Email is not null)
            {
                result[id] = user.Email;
            }
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, bool>> GetTwoFactorEnabledByIdsAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        // Sequential, like the email lookup above: userManager runs on the request's
        // scoped DbContext, and EF Core throws "A second operation was started on this
        // context instance" if two of these overlap.
        var result = new Dictionary<string, bool>();
        foreach (var id in userIds)
        {
            var user = await _userManager.FindByIdAsync(id);
            result[id] = user?.TwoFactorEnabled ?? false;
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<List<string>> FindUserIdsByEmailAsync(string emailSearch, CancellationToken ct)
    {
        var term = emailSearch.ToLower();
        return await Task.FromResult(
            _userManager.Users
                .Where(u => u.Email != null && u.Email.ToLower().Contains(term))
                .Select(u => u.Id)
                .ToList());
    }

    /// <inheritdoc/>
    public async Task<string?> GetPrimaryRoleAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault();
    }

    /// <inheritdoc/>
    public Task<bool> AnyUsersExistAsync(CancellationToken ct)
    {
        return Task.FromResult(_userManager.Users.Any());
    }

    /// <inheritdoc/>
    public async Task<string> GenerateEmailConfirmationTokenAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    /// <inheritdoc/>
    public async Task<bool> ConfirmEmailAsync(string email, string token, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    /// <inheritdoc/>
    public async Task<bool> IsEmailConfirmedAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.EmailConfirmed ?? false;
    }

    /// <inheritdoc/>
    public async Task<(List<UserListItemDto> Items, int TotalCount)> ListUsersAsync(
        int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        // Batch-lookup linked client IDs. Was IClientRepository.FindClientIdsByUserIdsAsync --
        // read-only, so no tracking is needed.
        var userIds = users.Select(u => u.Id).ToList();
        var clientMap = await _db.Clients
            .AsNoTracking()
            .Where(c => userIds.Contains(c.UserId))
            .ToDictionaryAsync(c => c.UserId, c => c.Id, ct);

        var items = users.Select(u => new UserListItemDto(
            u.Id, clientMap.GetValueOrDefault(u.Id), u.FirstName, u.LastName,
            u.Email!, u.Language, u.LastLoginAt, u.CreatedAt)).ToList();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<UserDetailDto?> GetUserWithAccountsAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId, ct);
        var accounts = client is not null
            ? new List<UserAccountDto>
            {
                new(client.Id, client.FirstName, client.LastName, client.CompanyName, true)
            }
            : new List<UserAccountDto>();

        return new UserDetailDto(
            user.Id, user.FirstName, user.LastName, user.Email!,
            user.Language, user.LastLoginAt, user.CreatedAt, accounts);
    }

    /// <inheritdoc/>
    public async Task UpdateUserAsync(
        string userId, string firstName, string lastName, string email, string? language, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.UserName = email;
        user.Language = language;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }
    }

    /// <inheritdoc/>
    public async Task DeleteUserAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        // Close the linked client account before deleting the user. Tracked, not AsNoTracking:
        // Close() is called on it below and saved in this same pass.
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.UserId == userId, ct);
        if (client is not null)
        {
            client.Close();
            await _uow.SaveChangesAsync(ct);
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user: {errors}");
        }
    }

    /// <inheritdoc/>
    public async Task<string> GeneratePasswordResetTokenAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    /// <inheritdoc/>
    public async Task ChangePasswordAsync(string userId, string newPassword, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to change password: {errors}");
        }

    }

    /// <inheritdoc/>
    public async Task<bool> ResetPasswordWithTokenAsync(string email, string token, string newPassword, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return false;
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    /// <inheritdoc/>
    public async Task UpdateLastLoginAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        user.LastLoginAt = DateTimeOffset.UtcNow;
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
        user.TwoFactorSecret = secret;
        await _userManager.UpdateAsync(user);
        return secret;
    }

    /// <inheritdoc/>
    public async Task<string?> GetTwoFactorSecretAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.TwoFactorSecret;
    }

    /// <inheritdoc/>
    public async Task EnableTwoFactorAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        await _userManager.SetTwoFactorEnabledAsync(user, true);
    }

    /// <inheritdoc/>
    public async Task DisableTwoFactorAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");
        user.TwoFactorSecret = null;
        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.UpdateAsync(user);
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyTwoFactorCodeAsync(string userId, string code, CancellationToken ct)
    {
        // Every door that checks a code (sign-in, enabling, disabling) comes through here, so the
        // limits live here. The attempt is taken before the code is looked at, so parallel guesses
        // cannot overshoot the cap; a correct code gives it back.
        if (!_codeGuard.TryBeginAttempt(userId))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user?.TwoFactorSecret is null)
        {
            return false;
        }

        var secretBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);
        var totp = new Totp(secretBytes);
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

    /// <inheritdoc/>
    public async Task<(string Id, string Email)?> FindBySsoSubjectAsync(string sub, CancellationToken ct)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.SsoSubjectId == sub, ct);
        return user is null ? null : (user.Id, user.Email!);
    }


    /// <inheritdoc/>
    public async Task<IList<string>> GetRolesAsync(string userId, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return [];
        }

        return await _userManager.GetRolesAsync(user);
    }
}
