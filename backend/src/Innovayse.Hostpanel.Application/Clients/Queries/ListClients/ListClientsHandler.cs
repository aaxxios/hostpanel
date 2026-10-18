using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Clients.Queries.ListClients;

/// <summary>
/// Returns a paginated list of clients with the details of the people behind them. Supports
/// filtering by name, email, phone and status.
/// </summary>
public sealed class ListClientsHandler
{
    /// <summary>How many address matches an email filter draws on. See the note at the call site.</summary>
    private const int EmailMatchLimit = 500;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the people behind the client rows.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Reads the people behind the client rows.</param>
    public ListClientsHandler(IHostpanelDbContext context, IIdentityProvider identity)
    {
        _context = context;
        _identity = identity;
    }

    /// <summary>Retrieves a paginated, filtered client list.</summary>
    /// <param name="query">The list query with pagination and filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The requested page of clients, never a refusal.</returns>
    public async Task<Result<PagedResult<ClientListItemDto>>> HandleAsync(ListClientsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var page = Math.Max(1, query.Page);

        // The address is not a column on the client row, so filtering by it means asking the
        // identity provider which subjects match and then filtering on those.
        //
        // Bounded, because the answer becomes an IN clause. A search matching more people than
        // this would build a query the database has to be talked into accepting, so the page is
        // drawn from the first EmailMatchLimit matches instead.
        IReadOnlyList<string>? emailUserIds = null;
        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var (matches, _) = await _identity.ListAsync(query.Email, 1, EmailMatchLimit, ct);
            emailUserIds = matches.Select(m => m.Subject).ToList();
        }

        ClientStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ClientStatus>(query.Status, true, out var parsed))
        {
            statusFilter = parsed;
        }

        var clients = _context.Clients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLower();
            clients = clients.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Phone))
        {
            var phoneTerm = query.Phone.ToLower();
            clients = clients.Where(c => c.Phone != null && c.Phone.ToLower().Contains(phoneTerm));
        }

        if (statusFilter is not null)
        {
            clients = clients.Where(c => c.Status == statusFilter.Value);
        }

        if (emailUserIds is not null)
        {
            clients = clients.Where(c => emailUserIds.Contains(c.UserId));
        }

        var totalCount = await clients.CountAsync(ct);

        var items = await clients
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Paginate(page, pageSize)
            .ToListAsync(ct);

        // One lookup for the whole page, rather than one per row.
        var accounts = await _identity.GetAccountsBySubjectsAsync(items.Select(c => c.UserId).Distinct(), ct);

        var dtos = items.Select(c => MapToListItem(c, accounts)).ToList();

        return Result<PagedResult<ClientListItemDto>>.Success(new PagedResult<ClientListItemDto>(dtos, totalCount, page, pageSize));
    }

    /// <summary>Maps a <see cref="Client"/> to <see cref="ClientListItemDto"/>.</summary>
    /// <param name="client">The client to map.</param>
    /// <param name="accounts">Subject to account lookup for this page.</param>
    /// <returns>The list item DTO.</returns>
    private static ClientListItemDto MapToListItem(Client client, IReadOnlyDictionary<string, IdentityAccount> accounts)
    {
        // A client row whose subject resolves to nobody is flagged rather than hidden: it is how a
        // legacy or deleted account shows up, and the screen has a column for it.
        var account = accounts.GetValueOrDefault(client.UserId);

        return new ClientListItemDto(
            client.Id, client.UserId, account?.Email ?? string.Empty,
            client.FirstName, client.LastName, client.CompanyName, client.Status,
            account is null, account?.TwoFactorEnabled ?? false, client.CreatedAt);
    }
}
