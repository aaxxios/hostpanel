using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Authorization;

/// <summary>
/// Resolves the caller's account and permissions from the client records and the links stored beside them.
/// </summary>
public sealed class CallerClientResolver : ICallerClientResolver
{
    /// <summary>The module's data context, for the owner record and the stored links.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking. Nothing tells this type whose account to resolve.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The module's data context, for the owner record and the stored links.</param>
    /// <param name="caller">Who is asking. Nothing tells this type whose account to resolve.</param>
    public CallerClientResolver(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task<CallerClientAccess> ResolveAsync(CancellationToken ct)
    {
        var userId = _caller.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return CallerClientAccess.None;
        }

        // The account owner is the user the client record itself names; it is never also a stored link.
        var ownedId = await _context.Clients.AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);
        if (ownedId is not null)
        {
            return new CallerClientAccess(ownedId, true, ClientPermission.All);
        }

        var links = await _context.ClientUsers.AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new { u.ClientId, u.Permissions })
            .ToListAsync(ct);
        if (links.Count == 0)
        {
            return CallerClientAccess.None;
        }

        // Start from everything and keep only what every link grants. Masking with All first drops a
        // bit this product never defined, so an unknown flag in the table can only ever grant nothing.
        var common = ClientPermission.All;
        foreach (var link in links)
        {
            common &= link.Permissions & ClientPermission.All;
        }

        // With several links nothing says which account is meant, so the caller acts for none.
        int? clientId = links.Count == 1 ? links[0].ClientId : null;

        return new CallerClientAccess(clientId, false, common);
    }
}
