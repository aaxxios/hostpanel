using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Extensions;

/// <summary>
/// Reads about a product's feature lines. Shared by the create and update handlers so the
/// label rule, and its case-insensitive comparison, lives once.
/// </summary>
/// <remarks>
/// The storefront's comparison table keys its rows on the label and takes the first value it
/// finds, so a second line under the same label renders nowhere: an operator would add or rename
/// one, see no change, and have nothing to explain why.
/// </remarks>
public static class ProductFeatureQueries
{
    /// <summary>
    /// Whether the product already describes a feature under this label, compared without regard
    /// to case because the storefront groups its comparison rows by the label as written.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="productId">Product to look in.</param>
    /// <param name="label">Label to look for.</param>
    /// <param name="excludingId">Line to ignore, so renaming a line does not clash with itself.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when another line already uses the label.</returns>
    public static async Task<bool> ProductFeatureLabelExistsAsync(
        this IHostpanelDbContext context, int productId, string label, int? excludingId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(label);

        var needle = label.Trim().ToLower();

        return await context.ProductFeatures.AnyAsync(
            f => f.ProductId == productId
                && f.Label.ToLower() == needle
                && (excludingId == null || f.Id != excludingId),
            ct);
    }
}
