namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// Reads people from wherever they live — this product's own database, or the SSO.
///
/// <para>
/// Read-only on purpose. In SSO mode this product stores no person records at all, so
/// there is no create, no update and no delete to offer; an interface that had them would
/// need half its surface to throw. Registration, passwords and profile edits belong to
/// whoever owns the accounts.
/// </para>
/// </summary>
public interface IIdentityProvider
{
    /// <summary>The person with this subject, or null if the provider does not know them.</summary>
    Task<IdentityAccount?> FindBySubjectAsync(string subject, CancellationToken ct);

    /// <summary>The person with this email address, or null.</summary>
    Task<IdentityAccount?> FindByEmailAsync(string email, CancellationToken ct);

    /// <summary>
    /// Email addresses for many subjects at once, keyed by subject. Subjects the provider
    /// does not know are absent rather than null-valued.
    ///
    /// <para>
    /// Exists because the admin screens resolve a page of client rows at a time, and doing
    /// that one lookup per row is a round trip per row — over the SSO's HTTP API, that is
    /// the difference between one request and fifty.
    /// </para>
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> GetEmailsBySubjectsAsync(
        IEnumerable<string> subjects, CancellationToken ct);

    /// <summary>
    /// Full accounts for many subjects at once, keyed by subject. Subjects the provider
    /// does not know are absent.
    ///
    /// <para>
    /// For the same reason as <see cref="GetEmailsBySubjectsAsync"/>, and used where a page
    /// of rows needs more than the address — the client list shows each row's 2FA status,
    /// which needs the whole account, not just the email.
    /// </para>
    /// </summary>
    Task<IReadOnlyDictionary<string, IdentityAccount>> GetAccountsBySubjectsAsync(
        IEnumerable<string> subjects, CancellationToken ct);

    /// <summary>One page of people, optionally filtered, with the unpaged total.</summary>
    Task<(IReadOnlyList<IdentityAccount> Items, int Total)> ListAsync(
        string? search, int page, int pageSize, CancellationToken ct);
}
