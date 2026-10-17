using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Users.Queries.ListUsers;

/// <summary>
/// Handles <see cref="ListUsersQuery"/>: pages through the configured identity provider and
/// joins each row to its linked client account, if any.
/// </summary>
public sealed class ListUsersHandler
{
    /// <summary>Reads people from wherever they live.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>The module's data context, for the accounts each person owns.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="identity">Reads people from wherever they live.</param>
    /// <param name="context">The module's data context, for the accounts each person owns.</param>
    public ListUsersHandler(IIdentityProvider identity, IHostpanelDbContext context)
    {
        _identity = identity;
        _context = context;
    }

    /// <summary>Returns one page of user summaries with the unpaged total.</summary>
    /// <param name="query">Search term and paging.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the page. This use case refuses nothing.</returns>
    public async Task<Result<PagedResult<UserListItemDto>>> HandleAsync(ListUsersQuery query, CancellationToken ct)
    {
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(1, query.Page);

        var (accounts, total) = await _identity.ListAsync(query.Search, page, pageSize, ct);

        // Was IClientRepository.FindClientIdsByUserIdsAsync -- one lookup for the page rather than
        // one per row.
        var subjects = accounts.Select(a => a.Subject).ToList();
        var clientIds = await _context.Clients
            .AsNoTracking()
            .Where(c => subjects.Contains(c.UserId))
            .ToDictionaryAsync(c => c.UserId, c => c.Id, ct);

        var items = accounts.Select(a => new UserListItemDto(
            a.Subject,
            clientIds.TryGetValue(a.Subject, out var clientId) ? clientId : null,
            a.FirstName, a.LastName, a.Email,
            // Language and the account's own creation date belong to whichever store holds
            // the person. A local deployment holds the language on its own account row, so it
            // is answered here; an SSO hands out neither, so both stay null. Left unanswered
            // rather than guessed at: a fabricated date on an admin screen is worse than a
            // blank one.
            a.Language, a.LastLoginAt, CreatedAt: default)).ToList();

        var page1 = new PagedResult<UserListItemDto>(items, total, page, pageSize);
        return Result<PagedResult<UserListItemDto>>.Success(page1);
    }
}
