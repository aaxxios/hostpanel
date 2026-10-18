using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;
/// <summary>
/// Creates and edits the records that describe people.
///
/// <para>
/// Separate from <see cref="IIdentityProvider"/>, which only reads, because only one of
/// the two deployments may do this at all. Where this product owns its users it creates
/// and edits them; where an SSO owns them it must not, and every implementation of this
/// interface in that mode refuses.
/// </para>
///
/// <para>
/// Refusing rather than omitting the interface is deliberate. A flow that still tries to
/// create a person in SSO mode is a flow nobody has finished thinking about, and it should
/// say so where it happens — loudly, naming what to do instead — rather than fail to
/// resolve at start-up in a stack trace that names dependency injection.
/// </para>
///
/// <para>
/// Every method answers a <see cref="Result"/> rather than throwing where an SSO owns the
/// accounts: the refusal is an expected outcome every caller must handle, and a
/// <c>HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_*</c> code names exactly which flow was
/// refused, so the caller chooses the code without a per-operation exception type to catch.
/// </para>
/// </summary>
public interface IUserProvisioning
{
    /// <summary>
    /// Creates a person and returns their subject.
    /// </summary>
    /// <param name="email">The address they will sign in with.</param>
    /// <param name="password">
    /// The password to set. Required: every flow that reaches this either collects one
    /// from an operator or generates one, and an account created without a password is one
    /// nobody can sign in to and nobody notices until they try.
    /// </param>
    /// <param name="firstName">Given name, or null to derive one from the address.</param>
    /// <param name="lastName">Family name, or null for none.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new subject, or a failed <see cref="Result{T}"/> carrying
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CREATE_ACCOUNT</c> where an SSO owns the accounts.
    /// </returns>
    Task<Result<string>> CreateAsync(
        string email, string password, string? firstName, string? lastName, CancellationToken ct);

    /// <summary>
    /// Updates a person's name and preferred UI language.
    /// </summary>
    /// <param name="subject">Whose profile to update.</param>
    /// <param name="firstName">The new given name.</param>
    /// <param name="lastName">The new family name.</param>
    /// <param name="language">Preferred UI language, or null to leave it unset.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or a failed <see cref="Result"/> carrying
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME</c> where an SSO owns the accounts.
    /// </returns>
    Task<Result> UpdateProfileAsync(
        string subject, string firstName, string lastName, string? language, CancellationToken ct);

    /// <summary>
    /// Changes the address a person signs in with.
    /// </summary>
    /// <remarks>
    /// Its own method, not a field on <see cref="UpdateProfileAsync"/>. The address is the
    /// credential half of an account rather than a detail on it: changing it changes who
    /// can sign in, it has to stay unique across accounts, and in time it will want a
    /// confirmation step that renaming somebody plainly does not. A caller editing a
    /// display name should not be able to move an account to a different address by
    /// passing the field it happened to read a moment ago.
    /// </remarks>
    /// <param name="subject">Whose sign-in address to change.</param>
    /// <param name="email">The new address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or a failed <see cref="Result"/> carrying
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_EMAIL</c> where an SSO owns the accounts.
    /// </returns>
    Task<Result> ChangeEmailAsync(string subject, string email, CancellationToken ct);

    /// <summary>
    /// Deletes an account. Records that reference the subject are left alone.
    /// </summary>
    /// <param name="subject">Whose account to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or a failed <see cref="Result"/> carrying
    /// <c>USER_PROVISIONING_NOT_ALLOWED_DELETE_ACCOUNT</c> where an SSO owns the accounts.
    /// </returns>
    Task<Result> DeleteAsync(string subject, CancellationToken ct);

    /// <summary>
    /// Sets a password directly, without the holder's involvement.
    /// </summary>
    /// <param name="subject">Whose password to set.</param>
    /// <param name="password">The new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> carrying <see langword="true"/> when the subject was
    /// found and the password set, or <see langword="false"/> when no such subject exists here --
    /// returned rather than a failure, because "not found" is an answer the caller
    /// (<c>SetUserPasswordHandler</c>) turns into its own <c>Result</c> code. A failed
    /// <see cref="Result{T}"/> carries <c>USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD</c> where an
    /// SSO owns the accounts.
    /// </returns>
    Task<Result<bool>> SetPasswordAsync(string subject, string password, CancellationToken ct);

    /// <summary>
    /// Issues a token the holder can use to choose a new password.
    /// </summary>
    /// <param name="subject">Whose reset token to issue.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> carrying the token, to be embedded in a reset link, or
    /// <see langword="null"/> when no such subject exists here -- returned rather than a failure,
    /// for the same reason as <see cref="SetPasswordAsync"/>'s <see langword="bool"/>. A failed
    /// <see cref="Result{T}"/> carries <c>USER_PROVISIONING_NOT_ALLOWED_ISSUE_PASSWORD_RESET</c>
    /// where an SSO owns the accounts.
    /// </returns>
    Task<Result<string?>> IssuePasswordResetTokenAsync(string subject, CancellationToken ct);

    /// <summary>
    /// Sets a new password using a token issued by <see cref="IssuePasswordResetTokenAsync"/>.
    /// </summary>
    /// <param name="email">The account's email address.</param>
    /// <param name="token">The reset token from the email link.</param>
    /// <param name="newPassword">The password to set.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> carrying <see langword="true"/> when the password was
    /// changed, or <see langword="false"/> when no such account exists or the token is invalid or
    /// expired. A failed <see cref="Result{T}"/> carries <c>USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD</c>
    /// where an SSO owns the accounts.
    /// </returns>
    Task<Result<bool>> ResetPasswordWithTokenAsync(string email, string token, string newPassword, CancellationToken ct);

    /// <summary>
    /// Marks a person's email address as confirmed without a mailed token.
    /// </summary>
    /// <remarks>
    /// For flows that already prove the holder controls the address or the installation, such as
    /// consuming the one-time setup token. Ordinary registration must keep using the token flow.
    /// </remarks>
    /// <param name="subject">Whose address to confirm.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success (also when no such subject exists here, matching the other writes), or a failed
    /// <see cref="Result"/> carrying <c>USER_PROVISIONING_NOT_ALLOWED_CONFIRM_EMAIL</c> where an
    /// SSO owns the accounts.
    /// </returns>
    Task<Result> ConfirmEmailAsync(string subject, CancellationToken ct);

    /// <summary>
    /// Issues a token that confirms a person's email address when it comes back through
    /// <c>POST /api/auth/confirm-email</c>.
    /// </summary>
    /// <param name="subject">Whose address the token confirms.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> carrying the token, to be embedded in a verification
    /// link, or <see langword="null"/> when no such subject exists here -- returned rather than a
    /// failure, for the same reason as <see cref="IssuePasswordResetTokenAsync"/>. A failed
    /// <see cref="Result{T}"/> carries <c>USER_PROVISIONING_NOT_ALLOWED_ISSUE_EMAIL_CONFIRMATION</c>
    /// where an SSO owns the accounts.
    /// </returns>
    Task<Result<string?>> IssueEmailConfirmationTokenAsync(string subject, CancellationToken ct);
}
