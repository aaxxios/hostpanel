using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IActivityLogger"/>: adds the line to the scoped context the
/// calling handler saves, stamped with the current caller.
/// </summary>
public sealed class ActivityLogger : IActivityLogger
{
    /// <summary>The application DbContext, the same scoped instance the calling handler saves.</summary>
    private readonly AppDbContext _db;

    /// <summary>The current caller, whose identity and IP stamp the line.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes a new instance of the <see cref="ActivityLogger"/> class.</summary>
    /// <param name="db">The application DbContext, the same scoped instance the calling handler saves.</param>
    /// <param name="caller">The current caller, whose identity and IP stamp the line.</param>
    public ActivityLogger(AppDbContext db, ICurrentUserService caller)
    {
        _db = db;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task RecordAsync(ActivityLogEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var userName = await _caller.GetUserNameAsync(ct);
        _db.ActivityLogs.Add(ActivityLog.Create(
            entry.ClientId,
            entry.Description,
            _caller.UserId,
            userName,
            _caller.Email,
            _caller.IpAddress));
    }
}
