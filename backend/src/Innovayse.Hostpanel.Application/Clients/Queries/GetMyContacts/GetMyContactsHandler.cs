using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyContacts;

/// <summary>Reads the additional contacts of the account the caller acts for, owner or linked user.</summary>
public sealed class GetMyContactsHandler
{
    /// <summary>The module's data context, for the account's contacts.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the account's contacts.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public GetMyContactsHandler(IHostpanelDbContext context, IClientAccess access)
    {
        _context = context;
        _access = access;
    }

    /// <summary>Handles <see cref="GetMyContactsQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The account's contacts, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record.
    /// </returns>
    public async Task<Result<IReadOnlyList<ContactDto>>> HandleAsync(GetMyContactsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<IReadOnlyList<ContactDto>>.FailureCode(clientId.ErrorCode!);
        }

        // Read-only, so no tracking is needed.
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.ClientId == clientId.Value)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        return Result<IReadOnlyList<ContactDto>>.Success(contacts.Select(c => c.ToContactDto()).ToList());
    }
}
