using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// The sign-on provider's own TOTP endpoints, as this product calls them on behalf of the signed-in
/// caller.
///
/// <para>
/// Every call carries the caller's own bearer token, never the service key: two-factor is a
/// self-service action on the signed-in person's own account, not something this product
/// performs on its own authority, and the sign-on provider's TOTP endpoints are shaped to match; they
/// authenticate the same way hostpanel's own <c>[Authorize]</c> endpoints already do in this
/// mode. Passed as a parameter on every call rather than set as a default header on the
/// client, because unlike <see cref="SsoServiceClient"/> this client has no single fixed
/// credential to attach at registration time.
/// </para>
/// </summary>
public sealed class SsoTwoFactorClient
{
    /// <summary>The sign-on provider's TOTP client, base address and resilience profile set at registration.</summary>
    private readonly HttpClient _http;

    /// <summary>Initializes a new instance of the <see cref="SsoTwoFactorClient"/> class.</summary>
    /// <param name="http">The sign-on provider's TOTP client, base address and resilience profile set at registration.</param>
    public SsoTwoFactorClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>Starts enrolment and returns the secret and QR URI to show the caller.</summary>
    /// <param name="bearerToken">The caller's own access token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The pending secret and its QR URI.</returns>
    public async Task<SsoTotpEnrolment> EnableAsync(string bearerToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/totp/enable");
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken);

        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<SsoTotpEnrolmentWire>(ct)
            ?? throw new InvalidOperationException("SSO returned an empty TOTP enrolment response.");
        return new SsoTotpEnrolment(body.Secret, body.QrUri);
    }

    /// <summary>
    /// Submits a TOTP code to complete enrolment (or, on the sign-on provider's own re-auth challenge
    /// path, to satisfy it: hostpanel only ever calls this to complete enrolment).
    /// </summary>
    /// <param name="bearerToken">The caller's own access token.</param>
    /// <param name="totpCode">The six digits the authenticator app currently shows.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The verification result, or null when the sign-on provider rejected the code as not matching.
    /// </returns>
    public async Task<SsoTotpVerification?> VerifyAsync(string bearerToken, string totpCode, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/totp/verify")
        {
            Content = JsonContent.Create(new SsoTotpCodeWire(totpCode)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken);

        using var response = await _http.SendAsync(request, ct);
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<SsoTotpVerificationWire>(ct);
        return new SsoTotpVerification(body?.BackupCodes);
    }

    /// <summary>Switches two-factor off, once <paramref name="totpCode"/> proves the caller still holds it.</summary>
    /// <param name="bearerToken">The caller's own access token.</param>
    /// <param name="totpCode">A current code from the authenticator app.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when it was switched off; false when the sign-on provider rejected the code.</returns>
    public async Task<bool> DisableAsync(string bearerToken, string totpCode, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/account/totp/disable")
        {
            Content = JsonContent.Create(new SsoTotpCodeWire(totpCode)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, bearerToken);

        using var response = await _http.SendAsync(request, ct);
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }
}
