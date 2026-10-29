using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An in-memory <see cref="IIdentityProvider"/> for tests that need to control, and count, subject
/// lookups without a user store behind them. A fake port, not a mock: it answers from the account it
/// was given and records how often it was asked.
/// </summary>
/// <param name="account">The account every subject resolves to; null makes every subject unknown.</param>
public sealed class FakeIdentityProvider(IdentityAccount? account) : IIdentityProvider
{
    /// <summary>How many times <see cref="FindBySubjectAsync"/> has been called.</summary>
    public int SubjectLookups { get; private set; }

    /// <inheritdoc />
    public Task<IdentityAccount?> FindBySubjectAsync(string subject, CancellationToken ct)
    {
        SubjectLookups++;
        return Task.FromResult(account);
    }

    /// <inheritdoc />
    public Task<IdentityAccount?> FindByEmailAsync(string email, CancellationToken ct)
    {
        return Task.FromResult(account is not null && account.Email == email ? account : null);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, IdentityAccount>> GetAccountsBySubjectsAsync(
        IEnumerable<string> subjects, CancellationToken ct)
    {
        IReadOnlyDictionary<string, IdentityAccount> result = account is null
            ? new Dictionary<string, IdentityAccount>()
            : subjects.ToDictionary(subject => subject, _ => account);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<PaginatedList<IdentityAccount>> ListAsync(IdentityAccountListQuery query, CancellationToken ct)
    {
        List<IdentityAccount> items = account is null ? [] : [account];
        return Task.FromResult(new PaginatedList<IdentityAccount>(items, items.Count, query.Page, query.PageSize));
    }
}
