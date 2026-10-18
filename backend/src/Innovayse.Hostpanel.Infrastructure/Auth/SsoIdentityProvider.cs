using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Auth;
/// <summary>
/// Reads people from the SSO, which owns them.
///
/// <para>
/// Creates nothing and caches nothing. This product stores no person records in SSO mode,
/// so there is no local copy to write and none to go stale — which is the failure the
/// arrangement it replaces had: a shadow row written once at first sign-in and never
/// updated again.
/// </para>
/// </summary>
public sealed class SsoIdentityProvider : IIdentityProvider
{
    /// <summary>The sso this class uses.</summary>
    private readonly SsoServiceClient _sso;

    /// <summary>Initializes a new instance of the <see cref="SsoIdentityProvider"/> class.</summary>
    /// <param name="sso">The sso this class uses.</param>
    public SsoIdentityProvider(SsoServiceClient sso)
    {
        _sso = sso;
    }

    /// <inheritdoc/>
    public async Task<IdentityAccount?> FindBySubjectAsync(string subject, CancellationToken ct) =>
        Map(await _sso.GetByIdAsync(subject, ct));

    /// <inheritdoc/>
    public async Task<IdentityAccount?> FindByEmailAsync(string email, CancellationToken ct) =>
        Map(await _sso.GetByEmailAsync(email, ct));

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, string>> GetEmailsBySubjectsAsync(
        IEnumerable<string> subjects, CancellationToken ct)
    {
        var accounts = await _sso.GetBatchAsync(subjects.Distinct().ToList(), ct);
        return accounts.ToDictionary(kv => kv.Key, kv => kv.Value.Email);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, IdentityAccount>> GetAccountsBySubjectsAsync(
        IEnumerable<string> subjects, CancellationToken ct)
    {
        var accounts = await _sso.GetBatchAsync(subjects.Distinct().ToList(), ct);
        return accounts.ToDictionary(kv => kv.Key, kv => Map(kv.Value)!);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<IdentityAccount> Items, int Total)> ListAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var (items, total) = await _sso.ListAsync(search, page, pageSize, ct);
        return (items.Select(a => Map(a)!).ToList(), total);
    }

    // Language is left at its default null on purpose. Where the SSO owns the person it
    // owns their language too, and hostpanel's service view of an SSO account does not carry
    // one -- so there is nothing here to answer with, and answering "English" would be a
    // guess dressed up as a stored preference.
    /// <summary>Projects an SSO account onto the provider-neutral <see cref="IdentityAccount"/>.</summary>
    /// <param name="account">The account the SSO returned, or null when it knows none.</param>
    /// <returns>The projected account, or null when <paramref name="account"/> is null.</returns>
    private static IdentityAccount? Map(SsoServiceClient.SsoAccount? account) =>
        account is null ? null : new IdentityAccount(
            account.Subject, account.Email, account.FirstName, account.LastName, account.TwoFactorEnabled);
}
