using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Auth;
/// <summary>
/// The roles a subject holds, kept in this product's own database in both modes.
/// </summary>
public sealed class SubjectRoleStore : ISubjectRoleStore
{
    /// <summary>The db this class uses.</summary>
    private readonly AppDbContext _db;

    /// <summary>Initializes a new instance of the <see cref="SubjectRoleStore"/> class.</summary>
    /// <param name="db">The db this class uses.</param>
    public SubjectRoleStore(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetRolesAsync(string subject, CancellationToken ct) =>
        await _db.SubjectRoles
            .AsNoTracking()
            .Where(x => x.Subject == subject)
            .Select(x => x.Role)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(string subject, string role, CancellationToken ct)
    {
        // Checked rather than caught: a duplicate grant is an ordinary thing for a caller
        // to do, and letting it reach the database would surface as an exception the
        // caller then has to distinguish from a real failure.
        var held = await _db.SubjectRoles
            .AnyAsync(x => x.Subject == subject && x.Role == role, ct);
        if (held)
        {
            return;
        }

        _db.SubjectRoles.Add(new SubjectRole { Subject = subject, Role = role });
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string subject, string role, CancellationToken ct)
    {
        await _db.SubjectRoles
            .Where(x => x.Subject == subject && x.Role == role)
            .ExecuteDeleteAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> AnyHasRoleAsync(string role, CancellationToken ct) =>
        await _db.SubjectRoles.AnyAsync(x => x.Role == role, ct);
}
