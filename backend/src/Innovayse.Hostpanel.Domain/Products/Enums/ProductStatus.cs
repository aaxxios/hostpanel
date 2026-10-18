namespace Innovayse.Hostpanel.Domain.Products.Enums;
/// <summary>Lifecycle status of a product listing.</summary>
public enum ProductStatus
{
    /// <summary>Product is available for ordering.</summary>
    Active = 0,

    /// <summary>Product is hidden and cannot be ordered.</summary>
    Inactive = 1,
}
