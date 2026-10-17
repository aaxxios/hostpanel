using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Commands.DeleteProductFeature;

/// <summary>Removes a specification line.</summary>
public sealed class DeleteProductFeatureHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeleteProductFeatureHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The line to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PRODUCT_FEATURE_NOT_FOUND</c> when the id names no line.</returns>
    public async Task<Result> HandleAsync(DeleteProductFeatureCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var feature = await _context.ProductFeatures.FirstOrDefaultAsync(f => f.Id == cmd.Id, ct);
        if (feature is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PRODUCT_FEATURE_NOT_FOUND);
        }

        _context.ProductFeatures.Remove(feature);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
