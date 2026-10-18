using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Users.Queries.GetUser;

/// <summary>
/// Handles <see cref="GetUserQuery"/>: composes one person's identity record with the client
/// account linked to them, if any.
/// </summary>
public sealed class GetUserHandler
{
    /// <summary>Reads people from wherever they live.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>The module's data context, for the account this person owns.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="identity">Reads people from wherever they live.</param>
    /// <param name="context">The module's data context, for the account this person owns.</param>
    public GetUserHandler(IIdentityProvider identity, IHostpanelDbContext context)
    {
        _identity = identity;
        _context = context;
    }

    /// <summary>Returns the user.</summary>
    /// <param name="query">The subject to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The user detail, or <c>IDENTITY_USER_NOT_FOUND</c> when the identity provider does not know
    /// the subject.
    /// </returns>
    public async Task<Result<UserDetailDto>> HandleAsync(GetUserQuery query, CancellationToken ct)
    {
        var account = await _identity.FindBySubjectAsync(query.Id, ct);
        if (account is null)
        {
            return Result<UserDetailDto>.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
        }

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed. Contacts
        // and Users are not read here, so unlike the repository's Include-laden query, this one
        // stays a plain lookup: nothing below reads either navigation.
        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == query.Id, ct);
        var accounts = client is not null
            ? new List<UserAccountDto>
            {
                new(client.Id, client.FirstName, client.LastName, client.CompanyName, IsOwner: true),
            }
            : [];

        var dto = new UserDetailDto(
            account.Subject, account.FirstName, account.LastName, account.Email,
            // Language lives with whichever store holds the person: a local deployment
            // answers with the one on its own account row, an SSO-backed one answers null
            // because the SSO does not hand it out. Null rather than guessed at either way.
            account.Language, account.LastLoginAt,
            client?.CreatedAt ?? default, accounts);

        return Result<UserDetailDto>.Success(dto);
    }
}
