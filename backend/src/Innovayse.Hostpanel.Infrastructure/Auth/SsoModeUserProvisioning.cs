using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Infrastructure.Auth;
/// <summary>
/// What a deployment whose people belong to an SSO gets: a provisioner that refuses.
///
/// <para>
/// It exists so the refusal happens at the call, naming the flow that attempted it, rather
/// than as a missing registration at start-up. Nothing here writes, and nothing here can
/// be made to write — which is the guarantee the whole arrangement rests on.
/// </para>
/// </summary>
public sealed class SsoModeUserProvisioning : IUserProvisioning
{
    /// <inheritdoc/>
    public Task<Result<string>> CreateAsync(
        string email, string password, string? firstName, string? lastName, CancellationToken ct) =>
        Task.FromResult(Result<string>.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_CREATE_ACCOUNT));

    /// <inheritdoc/>
    public Task<Result> UpdateProfileAsync(
        string subject, string firstName, string lastName, string? language, CancellationToken ct) =>
        Task.FromResult(Result.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME));

    /// <inheritdoc/>
    public Task<Result> ChangeEmailAsync(string subject, string email, CancellationToken ct) =>
        Task.FromResult(Result.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_CHANGE_EMAIL));

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(string subject, CancellationToken ct) =>
        Task.FromResult(Result.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_DELETE_ACCOUNT));

    /// <inheritdoc/>
    public Task<Result<bool>> SetPasswordAsync(string subject, string password, CancellationToken ct) =>
        Task.FromResult(Result<bool>.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD));

    /// <inheritdoc/>
    public Task<Result<string?>> IssuePasswordResetTokenAsync(string subject, CancellationToken ct) =>
        Task.FromResult(
            Result<string?>.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_ISSUE_PASSWORD_RESET));

    /// <inheritdoc/>
    public Task<Result<bool>> ResetPasswordWithTokenAsync(
        string email, string token, string newPassword, CancellationToken ct) =>
        Task.FromResult(Result<bool>.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD));

    /// <inheritdoc/>
    public Task<Result> ConfirmEmailAsync(string subject, CancellationToken ct) =>
        Task.FromResult(Result.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_CONFIRM_EMAIL));

    /// <inheritdoc/>
    public Task<Result<string?>> IssueEmailConfirmationTokenAsync(string subject, CancellationToken ct) =>
        Task.FromResult(
            Result<string?>.FailureCode(HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_ISSUE_EMAIL_CONFIRMATION));
}
