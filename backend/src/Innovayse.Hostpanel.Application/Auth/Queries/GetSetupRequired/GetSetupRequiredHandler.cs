using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth.Enums;
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

    /// <summary>Initializes the handler.</summary>
    /// <param name="roles">Local role store; asked whether Admin is claimed.</param>
    public GetSetupRequiredHandler(ISubjectRoleStore roles)
    {
        _roles = roles;
    }

    /// <summary>Handles <see cref="GetSetupRequiredQuery"/>.</summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether setup is required.</returns>
    public async Task<Result<SetupRequiredDto>> HandleAsync(GetSetupRequiredQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var required = !await _roles.AnyHasRoleAsync(Role.Admin, ct);

        // This answer is anonymous, so it says nothing about the setup token itself. Claiming
        // setup is token-gated in every mode, so the caller needs no separate flag for it.
        return Result<SetupRequiredDto>.Success(new SetupRequiredDto(required));
    }
}
