using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Auth.Queries.GetAuthMode;

/// <summary>
/// Answers how this deployment signs people in, so a browser client can offer the right control
/// instead of guessing. Anonymous on purpose: the answer is a deployment shape, not a secret.
/// </summary>
public sealed class GetAuthModeHandler
{
    /// <summary>The wire value of a deployment that owns its own people.</summary>
    private const string LocalMode = "local";

    /// <summary>The wire value of a deployment whose people belong to the sign-on service.</summary>
    private const string SsoMode = "sso";

    /// <summary>Which sign-in mechanism this deployment runs.</summary>
    private readonly IAuthModeProvider _authMode;

    /// <summary>Initializes the handler.</summary>
    /// <param name="authMode">Which sign-in mechanism this deployment runs.</param>
    public GetAuthModeHandler(IAuthModeProvider authMode)
    {
        _authMode = authMode;
    }

    /// <summary>Handles <see cref="GetAuthModeQuery"/>.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The mode, <c>local</c> or <c>sso</c>.</returns>
    public Task<Result<AuthModeDto>> HandleAsync(GetAuthModeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        return Task.FromResult(Result<AuthModeDto>.Success(new AuthModeDto(_authMode.IsLocalMode ? LocalMode : SsoMode)));
    }
}
