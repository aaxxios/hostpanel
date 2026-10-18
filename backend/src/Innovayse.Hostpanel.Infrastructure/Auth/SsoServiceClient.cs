using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Infrastructure.Auth;
/// <summary>
/// The SSO's service API, as this product uses it.
///
/// <para>
/// Authenticated with the shared service key rather than a user's token: these calls
/// happen on behalf of the product, not of whoever is signed in. The equivalent admin
/// endpoints authenticate a superadmin through a cookie or bearer token and so cannot be
/// called by a machine at all.
/// </para>
///
/// <para>
/// A missing account comes back as null. Anything else — the SSO refusing the key, being
/// unreachable, or failing — throws. That distinction is the whole point of this class:
/// a lookup that could not happen must never be reported as "no such person", because
/// the caller would act on it as a deleted account.
/// </para>
/// </summary>
public sealed class SsoServiceClient
{
    /// <summary>The http this class uses.</summary>
    private readonly HttpClient _http;

    /// <summary>Initializes a new instance of the <see cref="SsoServiceClient"/> class.</summary>
    /// <param name="http">The http this class uses.</param>
    public SsoServiceClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>The account with this subject, or null if the SSO has no such account.</summary>
    /// <param name="subject">The SSO subject to look up.</param>
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
        var body = await response.Content.ReadFromJsonAsync<SsoUserResponse>(ct);
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

        var found = await response.Content.ReadFromJsonAsync<SsoLookupResponse>(ct);
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

        var body = await response.Content.ReadFromJsonAsync<SsoBatchResponse>(ct);
        if (body is null)
        {
            return new Dictionary<string, SsoAccount>();
        }

        return body.Users.ToDictionary(
            u => u.Id,
            u => new SsoAccount(u.Id, u.Email, u.FirstName, u.LastName, u.TwoFactorEnabled));
    }

    /// <summary>One page of accounts, with the unpaged total.</summary>
    public async Task<(IReadOnlyList<SsoAccount> Items, int Total)> ListAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = $"api/service/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        using var response = await _http.GetAsync(query, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<SsoListResponse>(ct);
        if (body is null)
        {
            return ([], 0);
        }

        return (body.Users.Select(u => new SsoAccount(u.Id, u.Email, u.FirstName, u.LastName, u.TwoFactorEnabled)).ToList(),
                body.Total);
    }

    /// <summary>An account as the SSO describes it.</summary>
    public sealed record SsoAccount(
        string Subject, string Email, string FirstName, string LastName, bool TwoFactorEnabled);

    /// <summary>Wire shape of one account answered by the SSO user endpoint.</summary>
    /// <param name="Email">The address the person signs in with.</param>
    /// <param name="FirstName">Given name.</param>
    /// <param name="LastName">Family name.</param>
    /// <param name="TwoFactorEnabled">Whether a second factor is set up on the account.</param>
    private sealed record SsoUserResponse(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("firstName")] string FirstName,
        [property: JsonPropertyName("lastName")] string LastName,
        [property: JsonPropertyName("twoFactorEnabled")] bool TwoFactorEnabled);

    /// <summary>Wire shape of the SSO answer to an email lookup.</summary>
    /// <param name="UserId">The SSO subject that owns the looked-up address.</param>
    private sealed record SsoLookupResponse(
        [property: JsonPropertyName("userId")] string UserId);

    /// <summary>Wire shape of the SSO answer to a batch lookup by subject.</summary>
    /// <param name="Users">The accounts the SSO knows among the requested subjects.</param>
    private sealed record SsoBatchResponse(
        [property: JsonPropertyName("users")] List<SsoListItem> Users);

    /// <summary>Wire shape of one page of the SSO account listing.</summary>
    /// <param name="Total">Number of matching accounts before paging.</param>
    /// <param name="Users">The accounts on the requested page.</param>
    private sealed record SsoListResponse(
        [property: JsonPropertyName("total")] int Total,
        [property: JsonPropertyName("users")] List<SsoListItem> Users);

    /// <summary>Wire shape of one account inside a batch or list answer.</summary>
    /// <param name="Id">The SSO subject of the account.</param>
    /// <param name="Email">The address the person signs in with.</param>
    /// <param name="FirstName">Given name.</param>
    /// <param name="LastName">Family name.</param>
    /// <param name="TwoFactorEnabled">Whether a second factor is set up on the account.</param>
    private sealed record SsoListItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("firstName")] string FirstName,
        [property: JsonPropertyName("lastName")] string LastName,
        [property: JsonPropertyName("twoFactorEnabled")] bool TwoFactorEnabled);
}
