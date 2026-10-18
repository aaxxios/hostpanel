using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Auth.Queries.GetMyAccount;

/// <summary>Resolves the caller's account through the configured provider and reads their local roles.</summary>
public sealed class GetMyAccountHandler
{
    /// <summary>Where this deployment's people are read from.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Local role store; roles are granted here, not by the SSO.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>
    /// Who is calling. Read through the port so that the two spellings of the subject claim stay in
    /// the one adapter that implements it.
    /// </summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="identity">Where this deployment's people are read from.</param>
    /// <param name="roles">Local role store; roles are granted here, not by the SSO.</param>
    /// <param name="caller">Who is calling.</param>
    public GetMyAccountHandler(IIdentityProvider identity, ISubjectRoleStore roles, ICurrentUserService caller)
    {
        _identity = identity;
        _roles = roles;
        _caller = caller;
    }

    /// <summary>Handles <see cref="GetMyAccountQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The caller's account, or <c>UNAUTHORIZED</c> when no subject or no account answers the credential.</returns>
    public async Task<Result<MyAccountDto>> HandleAsync(GetMyAccountQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var subject = _caller.UserId;
        if (subject is null)
        {
            return Result<MyAccountDto>.FailureCode(SharedErrorCodes.Unauthorized);
        }

        // EmailVerified: the provider's own answer wins (local mode reads the user store); null falls back to the token claim (SSO).
        // One lookup: both modes name a person the same way -- whatever the configured provider calls them.
        var account = await _identity.FindBySubjectAsync(subject, ct);
        if (account is null)
        {
            return Result<MyAccountDto>.FailureCode(SharedErrorCodes.Unauthorized);
        }

        var held = await _roles.GetRolesAsync(subject, ct);

        return Result<MyAccountDto>.Success(new MyAccountDto(account.Email, held, account.EmailVerified ?? _caller.IsEmailVerified));
    }
}
