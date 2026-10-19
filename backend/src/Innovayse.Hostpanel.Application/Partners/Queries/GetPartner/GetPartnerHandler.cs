using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Partners.Common;
using Innovayse.Hostpanel.Application.Partners.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Partners.Queries.GetPartner;

/// <summary>Loads a single partner with every translation and maps it to <see cref="PartnerAdminDto"/>.</summary>
public sealed class GetPartnerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetPartnerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The partner id to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The admin DTO, or <c>PARTNER_NOT_FOUND</c> when the id names no partner.</returns>
    public async Task<Result<PartnerAdminDto>> HandleAsync(GetPartnerQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await _context.Partners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (entity is null)
        {
            return Result<PartnerAdminDto>.FailureCode(HostpanelErrorCodes.PARTNER_NOT_FOUND);
        }

        return Result<PartnerAdminDto>.Success(entity.ToAdminDto());
    }
}
