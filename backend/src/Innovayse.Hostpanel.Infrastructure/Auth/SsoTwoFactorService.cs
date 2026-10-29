using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Infrastructure.Auth;

/// <summary>
/// single sign-on mode two-factor authentication: proxies to the sign-on provider's own, already-working TOTP
/// endpoints using the caller's own bearer token.
///
/// <para>
/// Not the service key: two-factor is a self-service action on the signed-in person's own
/// account, and the sign-on provider's TOTP endpoints authenticate exactly the way hostpanel's own
/// <c>[Authorize]</c> endpoints already do in this mode; a bearer token identifying the
/// person, not a credential identifying the platform. The token comes from
/// <see cref="ICallerAccessToken"/>; the request itself is read in the API, never here.
/// </para>
/// </summary>
public sealed class SsoTwoFactorService : ITwoFactorService
{
    /// <summary>The typed client that calls the sign-on provider's TOTP endpoints.</summary>
    private readonly SsoTwoFactorClient _client;

    /// <summary>The caller's own bearer token, read from the request by the API.</summary>
    private readonly ICallerAccessToken _callerToken;

    /// <summary>Initializes a new instance of the <see cref="SsoTwoFactorService"/> class.</summary>
    /// <param name="client">The typed client that calls the sign-on provider's TOTP endpoints.</param>
    /// <param name="callerToken">The caller's own bearer token, read from the request by the API.</param>
    public SsoTwoFactorService(SsoTwoFactorClient client, ICallerAccessToken callerToken)
    {
        _client = client;
        _callerToken = callerToken;
    }

    /// <inheritdoc/>
    public async Task<Result<TwoFactorSetupDto>> StartSetupAsync(string userId, CancellationToken ct)
    {
        var token = _callerToken.GetBearerToken();
        if (token is null)
        {
            return Result<TwoFactorSetupDto>.FailureCode(SharedErrorCodes.Unauthorized);
        }

        var enrolment = await _client.EnableAsync(token, ct);
        return Result<TwoFactorSetupDto>.Success(new TwoFactorSetupDto(enrolment.Secret, enrolment.QrUri));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The sign-on provider's verify response carries backup codes on success, which hostpanel
    /// has nowhere to show, so they are discarded here.
    /// </remarks>
    public async Task<Result> EnableAsync(TwoFactorConfirmation confirmation, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(confirmation);

        var token = _callerToken.GetBearerToken();
        var verified = token is not null && await _client.VerifyAsync(token, confirmation.Code, ct) is not null;
        return verified ? Result.Success() : Result.FailureCode(HostpanelErrorCodes.TWO_FACTOR_CODE_INVALID);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The current password is not read here: the sign-on provider owns the account and its
    /// password, and asks for its own proof on its disable path.
    /// </remarks>
    public async Task<Result> DisableAsync(TwoFactorRemoval removal, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(removal);

        var token = _callerToken.GetBearerToken();
        var disabled = token is not null && await _client.DisableAsync(token, removal.Code, ct);
        return disabled ? Result.Success() : Result.FailureCode(HostpanelErrorCodes.TWO_FACTOR_CODE_INVALID);
    }
}
