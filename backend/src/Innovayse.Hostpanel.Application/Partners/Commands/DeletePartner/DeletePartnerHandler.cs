using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Partners.Commands.DeletePartner;

/// <summary>Deletes a partner permanently from the persistence store; its translations go with it.</summary>
public sealed class DeletePartnerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeletePartnerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The partner to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PARTNER_NOT_FOUND</c> when the id names no partner.</returns>
    public async Task<Result> HandleAsync(DeletePartnerCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Partners.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PARTNER_NOT_FOUND);
        }

        _context.Partners.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
