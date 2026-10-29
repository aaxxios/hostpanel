using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartnerOrder;

/// <summary>
/// Applies the new sort order values for the ids listed in the command and persists the reordering in one
/// transaction. Partners whose ids are not in the command are left unchanged.
/// </summary>
public sealed class UpdatePartnerOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdatePartnerOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The partner id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PARTNER_NOT_FOUND</c> when an id names no row; nothing is moved then.</returns>
    public async Task<Result> HandleAsync(UpdatePartnerOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (!await _context.Partners.ApplySortOrderAsync(cmd.Items, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.PARTNER_NOT_FOUND);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
