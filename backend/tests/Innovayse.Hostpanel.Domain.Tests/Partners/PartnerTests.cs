using Innovayse.Hostpanel.Domain.Partners;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Partners;

/// <summary>Unit tests for the <see cref="Partner"/> aggregate's behaviour.</summary>
public sealed class PartnerTests
{
    /// <summary>Builds a partner with the sample first-state values.</summary>
    /// <returns>A new, active partner.</returns>
    private static Partner NewEntity() => Partner.Create(logoUrl: "/logo.png", url: "https://acme.example", sortOrder: 2, isActive: true);

    /// <summary>Create stores every field, starts with no translations and stamps the creation time.</summary>
    [Fact]
    public void Create_StoresFields_AndStartsWithoutTranslations()
    {
        var before = DateTimeOffset.UtcNow;

        var entity = NewEntity();

        Assert.Equal("/logo.png", entity.LogoUrl);
        Assert.Equal("https://acme.example", entity.Url);
        Assert.Equal(2, entity.SortOrder);
        Assert.True(entity.IsActive);
        Assert.Empty(entity.Translations);
        Assert.True(entity.CreatedAt >= before);
    }

    /// <summary>Update replaces every non-translation field and keeps the creation time.</summary>
    [Fact]
    public void Update_ReplacesEveryField()
    {
        var entity = NewEntity();
        var created = entity.CreatedAt;

        entity.Update(logoUrl: null, url: null, sortOrder: 7, isActive: false);

        Assert.Null(entity.LogoUrl);
        Assert.Null(entity.Url);
        Assert.Equal(7, entity.SortOrder);
        Assert.False(entity.IsActive);
        Assert.Equal(created, entity.CreatedAt);
    }

    /// <summary>Deactivate hides the partner and Activate shows it again.</summary>
    [Fact]
    public void DeactivateThenActivate_TogglesVisibility()
    {
        var entity = NewEntity();

        entity.Deactivate();
        Assert.False(entity.IsActive);

        entity.Activate();
        Assert.True(entity.IsActive);
    }

    /// <summary>SetSortOrder moves the partner in the display order and touches nothing else.</summary>
    [Fact]
    public void SetSortOrder_ChangesOnlyTheOrder()
    {
        var entity = NewEntity();

        entity.SetSortOrder(11);

        Assert.Equal(11, entity.SortOrder);
        Assert.True(entity.IsActive);
    }

    /// <summary>SetTranslation adds a locale once and updates it in place the second time.</summary>
    [Fact]
    public void SetTranslation_AddsThenUpdatesTheSameLocale()
    {
        var entity = NewEntity();

        entity.SetTranslation("en", name: "Acme");
        entity.SetTranslation("en", name: "Acme Ltd");

        var translation = Assert.Single(entity.Translations);
        Assert.Equal("en", translation.Locale);
        Assert.Equal("Acme Ltd", translation.Name);
    }

    /// <summary>RemoveTranslation drops only the named locale and ignores one that is absent.</summary>
    [Fact]
    public void RemoveTranslation_DropsOnlyThatLocale()
    {
        var entity = NewEntity();
        entity.SetTranslation("en", name: "Acme");
        entity.SetTranslation("hy", name: "Acme Ltd");

        entity.RemoveTranslation("en");
        entity.RemoveTranslation("ru");

        var remaining = Assert.Single(entity.Translations);
        Assert.Equal("hy", remaining.Locale);
    }

    /// <summary>Create refuses a negative sort order with <c>PartnerErrors.SortOrderNegative</c>.</summary>
    [Fact]
    public void Create_NegativeSortOrder_Throws()
    {
        var ex = Assert.Throws<DomainRuleException>(() => Partner.Create(null, null, -1, true));

        Assert.Equal(PartnerErrors.SortOrderNegative, ex.Code);
    }

    /// <summary>Update refuses a negative sort order and leaves the entity untouched.</summary>
    [Fact]
    public void Update_NegativeSortOrder_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update(null, null, -1, true));

        Assert.Equal(PartnerErrors.SortOrderNegative, ex.Code);
        Assert.Equal(2, entity.SortOrder);
    }

    /// <summary>SetSortOrder refuses a negative value and keeps the current order.</summary>
    [Fact]
    public void SetSortOrder_Negative_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.SetSortOrder(-1));

        Assert.Equal(PartnerErrors.SortOrderNegative, ex.Code);
        Assert.Equal(2, entity.SortOrder);
    }

    /// <summary>Zero is the lowest allowed sort order.</summary>
    [Fact]
    public void SetSortOrder_Zero_IsAllowed()
    {
        var entity = NewEntity();

        entity.SetSortOrder(0);

        Assert.Equal(0, entity.SortOrder);
    }
}
