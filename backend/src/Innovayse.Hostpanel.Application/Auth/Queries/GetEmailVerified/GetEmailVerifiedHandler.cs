using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Auth.Queries.GetEmailVerified;

/// <summary>
/// Reports whether the caller's email is confirmed: the user store's answer in local mode, the
/// credential's claim in SSO mode.
/// </summary>
public sealed class GetEmailVerifiedHandler
{
    /// <summary>
    /// Who is calling. Read through the port so that the two spellings of the subject claim stay in
    /// the one adapter that implements it.
    /// </summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Where this deployment's people are read from.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Whether this deployment owns its own people.</summary>
    private readonly IAuthModeProvider _mode;

    /// <summary>Initializes the handler.</summary>
    /// <param name="caller">Who is calling.</param>
    /// <param name="identity">Where this deployment's people are read from.</param>
    /// <param name="mode">Whether this deployment owns its own people.</param>
    public GetEmailVerifiedHandler(ICurrentUserService caller, IIdentityProvider identity, IAuthModeProvider mode)
    {
        _caller = caller;
        _identity = identity;
        _mode = mode;
    }

    /// <summary>Handles <see cref="GetEmailVerifiedQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The verification state, <c>null</c> when neither the store nor the credential answers.</returns>
    public async Task<Result<EmailVerifiedDto>> HandleAsync(GetEmailVerifiedQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // SSO is the identity, so its claim is the answer and no lookup is made. Locally the claim
        // is frozen at sign-in, so the user store is asked instead.
        var subject = _caller.UserId;
        if (!_mode.IsLocalMode || subject is null)
        {
            return Result<EmailVerifiedDto>.Success(new EmailVerifiedDto(_caller.IsEmailVerified));
        }

        var account = await _identity.FindBySubjectAsync(subject, ct);

        return Result<EmailVerifiedDto>.Success(new EmailVerifiedDto(account?.EmailVerified ?? _caller.IsEmailVerified));
    }
}
