using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IClientCredit"/>: changes the tracked client through its own
/// credit methods on the scoped context the calling handler saves.
/// </summary>
public sealed class ClientCredit : IClientCredit
{
    /// <summary>The application DbContext, the same scoped instance the calling handler saves.</summary>
    private readonly AppDbContext _db;

    /// <summary>Initializes a new instance of the <see cref="ClientCredit"/> class.</summary>
    /// <param name="db">The application DbContext, the same scoped instance the calling handler saves.</param>
    public ClientCredit(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<Result<ClientCreditBalance>> AddAsync(ClientCreditChange change, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(change);

        var client = await FindAsync(change.ClientId, ct);
        if (client is null)
        {
            return Result<ClientCreditBalance>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        client.AddCredit(change.Amount);
        return Result<ClientCreditBalance>.Success(new ClientCreditBalance(client.Id, client.CreditBalance));
    }

    /// <inheritdoc/>
    public async Task<Result<ClientCreditBalance>> DeductAsync(ClientCreditChange change, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(change);

        var client = await FindAsync(change.ClientId, ct);
        if (client is null)
        {
            return Result<ClientCreditBalance>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        if (change.Amount > client.CreditBalance)
        {
            return Result<ClientCreditBalance>.FailureCode(HostpanelErrorCodes.CLIENT_CREDIT_INSUFFICIENT);
        }

        client.DeductCredit(change.Amount);
        return Result<ClientCreditBalance>.Success(new ClientCreditBalance(client.Id, client.CreditBalance));
    }

    /// <inheritdoc/>
    public async Task<Result<ClientCreditBalance>> AddAndSaveAsync(ClientCreditChange change, CancellationToken ct)
    {
        var added = await AddAsync(change, ct);
        if (added.IsFailure)
        {
            return added;
        }

        var client = (await FindAsync(change.ClientId, ct))!;
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException ex)
            when (ex.InnerException is DbUpdateConcurrencyException inner
                && inner.Entries.All(e => ReferenceEquals(e.Entity, client)))
        {
            // Only the client row moved underneath: reload it and add once more on the fresh
            // balance. The failed save wrote nothing and the caller's other changes stay staged.
            await _db.Entry(client).ReloadAsync(ct);
            client.AddCredit(change.Amount);
            await _db.SaveChangesAsync(ct);
        }

        return Result<ClientCreditBalance>.Success(new ClientCreditBalance(client.Id, client.CreditBalance));
    }

    /// <summary>Finds the client, reusing the instance the context already tracks.</summary>
    /// <param name="clientId">The client id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked client, or null when the id names none.</returns>
    private async Task<Client?> FindAsync(int clientId, CancellationToken ct)
    {
        return await _db.Clients.FindAsync([clientId], ct);
    }
}
