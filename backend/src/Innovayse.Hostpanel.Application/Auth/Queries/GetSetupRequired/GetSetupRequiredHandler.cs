using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Auth.Queries.GetSetupRequired;

/// <summary>
/// Answers whether initial admin setup is required (nobody holds Admin yet) and whether a setup
/// token is needed to claim it.
/// </summary>
public sealed class GetSetupRequiredHandler
{
    /// <summary>Local role store; asked whether Admin is claimed.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>Which sign-in mechanism this deployment runs.</summary>
    private readonly IAuthModeProvider _authMode;

    /// <summary>Initializes the handler.</summary>
    /// <param name="roles">Local role store; asked whether Admin is claimed.</param>
    /// <param name="authMode">Which sign-in mechanism this deployment runs.</param>
    public GetSetupRequiredHandler(ISubjectRoleStore roles, IAuthModeProvider authMode)
    {
        _roles = roles;
        _authMode = authMode;
    }

    /// <summary>Handles <see cref="GetSetupRequiredQuery"/>.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether setup is required and whether it is token-gated.</returns>
    public async Task<Result<SetupRequiredDto>> HandleAsync(GetSetupRequiredQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var required = !await _roles.AnyHasRoleAsync(Roles.Admin, ct);

        // Deliberately says only *whether* a token is wanted, never anything about the token
        // itself -- this answer is anonymous, and it has to stay a statement about the
        // deployment's shape rather than about its secrets.
        return Result<SetupRequiredDto>.Success(new SetupRequiredDto(required, required && _authMode.IsLocalMode));
    }
}
