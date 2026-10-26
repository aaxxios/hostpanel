using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Authorization;

/// <summary>
/// Reads the caller's permissions from the client records and the links stored beside them.
/// </summary>
public sealed class ClientAccess : IClientAccess
{
    /// <summary>The module's data context, for the owner record and the stored links.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking. Nothing tells this type whose permissions to read.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the reader.</summary>
    /// <param name="context">The module's data context, for the owner record and the stored links.</param>
    /// <param name="caller">Who is asking. Nothing tells this type whose permissions to read.</param>
    public ClientAccess(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task<ClientPermission> GetCallerPermissionsAsync(CancellationToken ct)
    {
        var userId = _caller.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return ClientPermission.None;
        }

        // The account owner is the user the client record itself names; it is never also a stored link.
        var ownsARecord = await _context.Clients.AsNoTracking().AnyAsync(c => c.UserId == userId, ct);
        if (ownsARecord)
        {
            return ClientPermission.All;
        }

        var grants = await _context.ClientUsers.AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => u.Permissions)
            .ToListAsync(ct);
        if (grants.Count == 0)
        {
            return ClientPermission.None;
        }

        // Start from everything and keep only what every link grants. Masking with All first drops a
        // bit this product never defined, so an unknown flag in the table can only ever grant nothing.
        var common = ClientPermission.All;
        foreach (var grant in grants)
        {
            common &= grant & ClientPermission.All;
        }

        return common;
    }
}
