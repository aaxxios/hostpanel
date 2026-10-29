using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Products.Common;

/// <summary>
/// What an order needs to know about a product it is about to sell: its name, whether it is on
/// sale, and its stored prices in the currency the order bills in.
/// </summary>
/// <param name="Id">The product id.</param>
/// <param name="Name">The product's display name, snapshotted onto the order line.</param>
/// <param name="Status">The product's status; only an active product may be ordered.</param>
/// <param name="Price">The product's stored prices in the requested currency; a null cycle is not sold in it.</param>
public sealed record OrderableProductDto(int Id, string Name, ProductStatus Status, ProductPriceDto Price);
