using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Products.Events;
namespace Innovayse.Hostpanel.Domain.Tests.Products;
/// <summary>Unit tests for the <see cref="Product"/> aggregate.</summary>
public class ProductTests
{
    /// <summary>Create sets all properties and raises ProductCreatedEvent.</summary>
    [Fact]
    public void Create_SetsAllProperties_AndRaisesEvent()
    {
        var product = Product.Create(
            groupId: 1,
            name: "Starter",
            description: "Entry plan",
            website: null,
            slug: null,
            packageName: null,
            type: ProductType.SharedHosting,
            monthlyPrice: 5.99m,
            annualPrice: 59.99m);

        Assert.Equal(1, product.GroupId);
        Assert.Equal("Starter", product.Name);
        Assert.Equal(ProductType.SharedHosting, product.Type);
        Assert.Equal(ProductStatus.Active, product.Status);
        Assert.Equal(5.99m, product.MonthlyPrice);
        Assert.Equal(59.99m, product.AnnualPrice);
        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductCreatedEvent>(product.DomainEvents.Single());
    }

    /// <summary>Update changes name and prices.</summary>
    [Fact]
    public void Update_ChangesNameAndPrices()
    {
        var product = Product.Create(1, "Old", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        product.Update("New", "desc", null, null, null, 9m, 90m, null);
        Assert.Equal("New", product.Name);
        Assert.Equal(9m, product.MonthlyPrice);
        Assert.Equal(90m, product.AnnualPrice);
    }

    /// <summary>Deactivate changes status to Inactive.</summary>
    [Fact]
    public void Deactivate_ChangesStatus()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        product.Deactivate();
        Assert.Equal(ProductStatus.Inactive, product.Status);
    }

    /// <summary>Create with ServerGroupId sets the property.</summary>
    [Fact]
    public void Create_WithServerGroupId_SetsProperty()
    {
        var product = Product.Create(1, "Linked", null, null, null, null,
            ProductType.SharedHosting, 5m, 50m, serverGroupId: 42);

        Assert.Equal(42, product.ServerGroupId);
    }

    /// <summary>Create without ServerGroupId defaults to null.</summary>
    [Fact]
    public void Create_WithoutServerGroupId_DefaultsToNull()
    {
        var product = Product.Create(1, "Unlinked", null, null, null, null,
            ProductType.SharedHosting, 5m, 50m);

        Assert.Null(product.ServerGroupId);
    }

    /// <summary>Update with ServerGroupId changes the value.</summary>
    [Fact]
    public void Update_WithServerGroupId_ChangesValue()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        Assert.Null(product.ServerGroupId);

        product.Update("P", null, null, null, null, 5m, 50m, serverGroupId: 7);

        Assert.Equal(7, product.ServerGroupId);
    }

    /// <summary>A new product is neither featured nor ordered until the operator says so.</summary>
    [Fact]
    public void Create_Defaults_NotFeaturedAndSortOrderZero()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m);

        Assert.False(product.IsFeatured);
        Assert.Equal(0, product.SortOrder);
    }

    /// <summary>Create carries the featured flag and the sort order it is given.</summary>
    [Fact]
    public void Create_WithFeaturedAndSortOrder_SetsBoth()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m,
            isFeatured: true, sortOrder: 3);

        Assert.True(product.IsFeatured);
        Assert.Equal(3, product.SortOrder);
    }

    /// <summary>Feature and Unfeature toggle the recommended-plan mark.</summary>
    [Fact]
    public void FeatureAndUnfeature_ToggleTheFlag()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m);

        product.Feature();
        Assert.True(product.IsFeatured);

        product.Unfeature();
        Assert.False(product.IsFeatured);
    }

    /// <summary>SetSortOrder moves the product inside its group.</summary>
    [Fact]
    public void SetSortOrder_ChangesThePosition()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m);

        product.SetSortOrder(9);

        Assert.Equal(9, product.SortOrder);
    }

    /// <summary>Update applies the featured flag and sort order when given.</summary>
    [Fact]
    public void Update_WithFeaturedAndSortOrder_ChangesBoth()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m);

        product.Update("P", null, null, null, null, 5m, 50m, null, isFeatured: true, sortOrder: 4);

        Assert.True(product.IsFeatured);
        Assert.Equal(4, product.SortOrder);
    }

    /// <summary>Update leaves the featured flag and sort order alone when they are not given.</summary>
    [Fact]
    public void Update_WithoutFeaturedAndSortOrder_KeepsBoth()
    {
        var product = Product.Create(1, "P", null, null, null, null, ProductType.SharedHosting, 5m, 50m,
            isFeatured: true, sortOrder: 6);

        product.Update("P2", null, null, null, null, 5m, 50m, null);

        Assert.True(product.IsFeatured);
        Assert.Equal(6, product.SortOrder);
    }
}
