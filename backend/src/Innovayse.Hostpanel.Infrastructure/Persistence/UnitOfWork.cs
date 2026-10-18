using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;
/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// Delegates to <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(CancellationToken)"/>.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    /// <summary>The application DbContext.</summary>
    private readonly AppDbContext _db;

    /// <summary>Initializes a new instance of the <see cref="UnitOfWork"/> class.</summary>
    /// <param name="db">The application DbContext.</param>
    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Translate the EF-specific exception to a persistence-agnostic one — the
            // Application layer must not reference EF Core (see
            // rules/architectures/clean-architecture.md), so this is the only place allowed
            // to know that the underlying cause was an optimistic-concurrency conflict.
            // Shared/Infrastructure, not Application: it crosses to ExceptionHandlingMiddleware, which
            // answers it 409, and a type Application alone may define has no honest home there.
            throw new ConcurrencyConflictException(
                "Another writer modified one of the saved aggregates concurrently.", ex);
        }
    }

    /// <inheritdoc/>
    public void DetachAll()
    {
        foreach (var entry in _db.ChangeTracker.Entries().ToList())
        {
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }
    }
}
