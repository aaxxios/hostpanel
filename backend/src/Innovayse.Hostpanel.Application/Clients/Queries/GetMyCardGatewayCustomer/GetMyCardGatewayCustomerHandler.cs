using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetMyCardGatewayCustomer;

/// <summary>Handles <see cref="GetMyCardGatewayCustomerQuery"/>.</summary>
public sealed class GetMyCardGatewayCustomerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the account the caller acts for, owner or linked user; the query does not say, and must not.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user; the query does not say, and must not.</param>
    public GetMyCardGatewayCustomerHandler(IHostpanelDbContext context, IClientAccess access)
    {
        _context = context;
        _access = access;
    }

    /// <summary>Reads the card gateway customer of the account the caller acts for.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The link, with a <see langword="null"/> customer id when none was ever saved, or
    /// <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller acts for no account.
    /// </returns>
    public async Task<Result<CardGatewayCustomerDto>> HandleAsync(GetMyCardGatewayCustomerQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<CardGatewayCustomerDto>.FailureCode(clientId.ErrorCode!);
        }

        var customer = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Id == clientId.Value)
            .Select(c => new CardGatewayCustomerDto(c.Id, c.CardGatewayCustomerId))
            .FirstOrDefaultAsync(ct);
        if (customer is null)
        {
            return Result<CardGatewayCustomerDto>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        return Result<CardGatewayCustomerDto>.Success(customer);
    }
}
