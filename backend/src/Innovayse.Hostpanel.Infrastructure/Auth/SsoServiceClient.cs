using System.Net;
using System.Net.Http.Json;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// The sign-on provider's service API, as this product uses it.
///
/// <para>
/// Authenticated with the shared service key rather than a user's token: these calls
/// happen on behalf of the product, not of whoever is signed in. The equivalent admin
/// endpoints authenticate a superadmin through a cookie or bearer token and so cannot be
/// called by a machine at all.
/// </para>
///
/// <para>
/// A missing account comes back as null. Anything else, the sign-on provider refusing the key, being
/// unreachable, or failing, throws. That distinction is the whole point of this class:
/// a lookup that could not happen must never be reported as "no such person", because
/// the caller would act on it as a deleted account.
/// </para>
/// </summary>
public sealed class SsoServiceClient
{
    /// <summary>The sign-on provider's service API client, base address and resilience profile set at registration.</summary>
    private readonly HttpClient _http;

    /// <summary>Initializes a new instance of the <see cref="SsoServiceClient"/> class.</summary>
    /// <param name="http">The sign-on provider's service API client, base address and resilience profile set at registration.</param>
    public SsoServiceClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>The account with this subject, or null if the sign-on provider has no such account.</summary>
    /// <param name="subject">The sign-on provider subject to look up.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The account, or null.</returns>
    public async Task<SsoAccount?> GetByIdAsync(string subject, CancellationToken ct)
    {
        using var response = await _http.GetAsync($"api/service/users/{subject}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<SsoUserWire>(ct);
        return body is null ? null : new SsoAccount(subject, body.Email, body.FirstName, body.LastName, body.TwoFactorEnabled);
    }

    /// <summary>The account with this email address, or null.</summary>
    /// <param name="email">The address to look up.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The account, or null.</returns>
    public async Task<SsoAccount?> GetByEmailAsync(string email, CancellationToken ct)
    {
        using var response = await _http.GetAsync(
            $"api/service/users/lookup?email={Uri.EscapeDataString(email)}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var found = await response.Content.ReadFromJsonAsync<SsoLookupWire>(ct);
        if (found is null || string.IsNullOrEmpty(found.UserId))
        {
            return null;
        }

        // The lookup answers with an id alone, so the name and address come from a second
        // call. Two round trips, and worth it: the alternative is a lookup endpoint that
        // returns a whole account to every caller that only wanted to know it exists.
        return await GetByIdAsync(found.UserId, ct);
    }

    /// <summary>Accounts for many subjects at once, keyed by subject.</summary>
    /// <param name="subjects">The subjects to fetch.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>Accounts keyed by subject; unknown subjects are absent.</returns>
    public async Task<IReadOnlyDictionary<string, SsoAccount>> GetBatchAsync(
        IReadOnlyCollection<string> subjects, CancellationToken ct)
    {
        if (subjects.Count == 0)
        {
            return new Dictionary<string, SsoAccount>();
        }

        using var response = await _http.PostAsJsonAsync(
            "api/service/users/batch", new { ids = subjects }, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<SsoBatchWire>(ct);
        if (body is null)
        {
            return new Dictionary<string, SsoAccount>();
        }

        return body.Users.ToDictionary(
            u => u.Id,
            u => new SsoAccount(u.Id, u.Email, u.FirstName, u.LastName, u.TwoFactorEnabled));
    }

    /// <summary>One page of accounts, with the unpaged total.</summary>
    /// <param name="query">The search text and the page to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The page of accounts with the total across all pages.</returns>
    public async Task<PaginatedList<SsoAccount>> ListAsync(IdentityAccountListQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var path = $"api/service/users?page={query.Page}&pageSize={query.PageSize}";
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            path += $"&search={Uri.EscapeDataString(query.Search)}";
        }

        using var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<SsoListWire>(ct);
        if (body is null)
        {
            return new PaginatedList<SsoAccount>([], 0, query.Page, query.PageSize);
        }

        return new PaginatedList<SsoAccount>(
            body.Users.Select(u => new SsoAccount(u.Id, u.Email, u.FirstName, u.LastName, u.TwoFactorEnabled)).ToList(),
            body.Total,
            query.Page,
            query.PageSize);
    }
}
