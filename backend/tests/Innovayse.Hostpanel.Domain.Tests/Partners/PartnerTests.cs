using Innovayse.Hostpanel.Domain.Partners;

namespace Innovayse.Hostpanel.Domain.Tests.Partners;

/// <summary>Unit tests for the <see cref="Partner"/> aggregate's behaviour.</summary>
public sealed class PartnerTests
{
    /// <summary>Builds a partner with the sample first-state values.</summary>
    /// <returns>A new, active partner.</returns>
    private static Partner NewEntity() => Partner.Create(logoUrl: "/logo.png", url: "https://acme.example", sortOrder: 2, isActive: true);

    /// <summary>Create stores every field and starts with no translations.</summary>
    [Fact]
    public void Create_StoresFields_AndStartsWithoutTranslations()
    {
        var entity = NewEntity();

        Assert.Equal("/logo.png", entity.LogoUrl);
        Assert.Equal("https://acme.example", entity.Url);
        Assert.Equal(2, entity.SortOrder);
        Assert.True(entity.IsActive);
        Assert.Empty(entity.Translations);
    }

    /// <summary>Update replaces every non-translation field.</summary>
    [Fact]
    public void Update_ReplacesEveryField()
    {
        var entity = NewEntity();

        entity.Update(logoUrl: null, url: null, sortOrder: 7, isActive: false);

        Assert.Null(entity.LogoUrl);
        Assert.Null(entity.Url);
        Assert.Equal(7, entity.SortOrder);
        Assert.False(entity.IsActive);
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

    /// <summary>
    /// ReplaceTranslations leaves exactly the given locales: a locale it does not name is removed, a
    /// kept one is updated in place and a new one is added.
    /// </summary>
    [Fact]
    public void ReplaceTranslations_RemovesUnlisted_UpdatesKept_AddsNew()
    {
        var entity = NewEntity();
        entity.SetTranslation("en", name: "Acme");
        entity.SetTranslation("ru", name: "Acme Ru");

        entity.ReplaceTranslations(new Dictionary<string, PartnerTranslationContent>
        {
            ["en"] = new PartnerTranslationContent("Acme Ltd"),
            ["hy"] = new PartnerTranslationContent("Acme Hy"),
        });

        Assert.Equal(["en", "hy"], entity.Translations.Select(t => t.Locale).Order());
        Assert.Equal("Acme Ltd", entity.Translations.Single(t => t.Locale == "en").Name);
    }
}
