using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Auth.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// What a deployment whose people belong to an external sign-on provider gets for the local account
/// store: an account service with no accounts in it, which never touches Identity.
/// </summary>
/// <remarks>
/// <para>
/// It exists so the container is complete in every mode. The local sign-in, two-factor sign-in and
/// registration handlers depend on <see cref="IUserService"/>, and Wolverine generates their code at
/// image build time in the image's mode with service location refused, so a port that is missing in
/// sign-on mode stops code generation for the whole image. Their routes still answer 404 in this mode
/// (<c>LocalModeOnlyFilter</c>), so nothing here is reached in normal operation.
/// </para>
/// <para>
/// Every read answers the port's own "no such account" value, so a call that did get through is refused
/// the way an unknown account is: a password sign-in fails, no account is found, no password or code
/// matches, no session version is current. Every write throws <see cref="InvalidOperationException"/>
/// through <see cref="SsoModeRefusal"/>, the failure <see cref="IUserService.CreateAsync"/> documents,
/// because there is no account to write to.
/// </para>
/// </remarks>
public sealed class SsoUserService : IUserService
{
    /// <inheritdoc/>
    public Task<string> CreateAsync(CreateUserInput input, CancellationToken ct) =>
        throw SsoModeRefusal.LocalAccountsOnly(nameof(CreateAsync));

    /// <inheritdoc/>
    public Task AddToRoleAsync(string userId, string role, CancellationToken ct) =>
        throw SsoModeRefusal.LocalAccountsOnly(nameof(AddToRoleAsync));

    /// <inheritdoc/>
    public Task<PasswordSignInResult> SignInWithPasswordAsync(PasswordSignInInput input, CancellationToken ct) =>
        Task.FromResult(new PasswordSignInResult(PasswordSignInOutcome.Failed, null));

    /// <inheritdoc/>
    public Task<UserIdentity?> FindByIdAsync(string userId, CancellationToken ct) =>
        Task.FromResult<UserIdentity?>(null);

    /// <inheritdoc/>
    public Task<bool> IsEmailConfirmedAsync(string userId, CancellationToken ct) => Task.FromResult(false);

    /// <inheritdoc/>
    public Task UpdateLastLoginAsync(string userId, CancellationToken ct) =>
        throw SsoModeRefusal.LocalAccountsOnly(nameof(UpdateLastLoginAsync));

    /// <inheritdoc/>
    public Task<bool> IsTwoFactorEnabledAsync(string userId, CancellationToken ct) => Task.FromResult(false);

    /// <inheritdoc/>
    public Task<string> GenerateTwoFactorSecretAsync(string userId, CancellationToken ct) =>
        throw SsoModeRefusal.LocalAccountsOnly(nameof(GenerateTwoFactorSecretAsync));

    /// <inheritdoc/>
    public Task<bool> EnableTwoFactorAsync(string userId, string code, CancellationToken ct) =>
        Task.FromResult(false);

    /// <inheritdoc/>
    public Task DisableTwoFactorAsync(string userId, CancellationToken ct) =>
        throw SsoModeRefusal.LocalAccountsOnly(nameof(DisableTwoFactorAsync));

    /// <inheritdoc/>
    public Task<bool> VerifyTwoFactorCodeAsync(string userId, string code, CancellationToken ct) =>
        Task.FromResult(false);

    /// <inheritdoc/>
    public Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken ct) =>
        Task.FromResult(false);

    /// <inheritdoc/>
    public Task<string?> GetSessionVersionAsync(string userId, CancellationToken ct) =>
        Task.FromResult<string?>(null);

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<string>>([]);
}
