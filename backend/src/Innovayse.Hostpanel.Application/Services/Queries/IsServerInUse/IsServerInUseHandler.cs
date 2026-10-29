using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Queries.IsServerInUse;

/// <summary>
/// Handles <see cref="IsServerInUseQuery"/>: a server is in use while a client service or an SSH key
/// still points at it.
/// </summary>
public sealed class IsServerInUseHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public IsServerInUseHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The server to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when a service or an SSH key references the server. Never refuses.</returns>
    public async Task<Result<bool>> HandleAsync(IsServerInUseQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var inUse = await _context.ClientServices.AsNoTracking().AnyAsync(s => s.ServerId == query.ServerId, ct)
            || await _context.SshKeys.AsNoTracking().AnyAsync(k => k.ServerId == query.ServerId, ct);

        return Result<bool>.Success(inUse);
    }
}
