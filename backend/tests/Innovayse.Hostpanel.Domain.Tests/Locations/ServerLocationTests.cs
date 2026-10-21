using Innovayse.Hostpanel.Domain.Locations;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Locations;

/// <summary>Unit tests for the <see cref="ServerLocation"/> aggregate's behaviour.</summary>
public sealed class ServerLocationTests
{
    /// <summary>Builds a location with the sample first-state values.</summary>
    /// <returns>A new, active location.</returns>
    private static ServerLocation NewEntity() => ServerLocation.Create(countryCode: "de", sortOrder: 2, isActive: true);

    /// <summary>Create stores every field, upper-cases the country code, starts with no translations and stamps the creation time.</summary>
    [Fact]
    public void Create_StoresFields_AndStartsWithoutTranslations()
    {
        var before = DateTimeOffset.UtcNow;

        var entity = NewEntity();

        Assert.Equal("DE", entity.CountryCode);
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

        entity.Update(countryCode: "nl", sortOrder: 7, isActive: false);

        Assert.Equal("NL", entity.CountryCode);
        Assert.Equal(7, entity.SortOrder);
        Assert.False(entity.IsActive);
        Assert.Equal(created, entity.CreatedAt);
    }

    /// <summary>Deactivate hides the location and Activate shows it again.</summary>
    [Fact]
    public void DeactivateThenActivate_TogglesVisibility()
    {
        var entity = NewEntity();

        entity.Deactivate();
        Assert.False(entity.IsActive);

        entity.Activate();
        Assert.True(entity.IsActive);
    }

    /// <summary>SetSortOrder moves the location in the display order and touches nothing else.</summary>
    [Fact]
    public void SetSortOrder_ChangesOnlyTheOrder()
    {
        var entity = NewEntity();

        entity.SetSortOrder(11);

        Assert.Equal(11, entity.SortOrder);
        Assert.True(entity.IsActive);
        Assert.Equal("DE", entity.CountryCode);
    }

    /// <summary>SetTranslation adds a locale once and updates it in place the second time.</summary>
    [Fact]
    public void SetTranslation_AddsThenUpdatesTheSameLocale()
    {
        var entity = NewEntity();

        entity.SetTranslation("en", name: "Frankfurt One", city: "Frankfurt", latencyNote: null);
        entity.SetTranslation("en", name: "Frankfurt Two", city: null, latencyNote: "About 20 ms from central Europe");

        var translation = Assert.Single(entity.Translations);
        Assert.Equal("en", translation.Locale);
        Assert.Equal("Frankfurt Two", translation.Name);
        Assert.Null(translation.City);
        Assert.Equal("About 20 ms from central Europe", translation.LatencyNote);
    }

    /// <summary>RemoveTranslation drops only the named locale and ignores one that is absent.</summary>
    [Fact]
    public void RemoveTranslation_DropsOnlyThatLocale()
    {
        var entity = NewEntity();
        entity.SetTranslation("en", name: "Frankfurt One", city: null, latencyNote: null);
        entity.SetTranslation("hy", name: "Ֆրանկֆուրտ", city: null, latencyNote: null);

        entity.RemoveTranslation("en");
        entity.RemoveTranslation("ru");

        var remaining = Assert.Single(entity.Translations);
        Assert.Equal("hy", remaining.Locale);
    }

    /// <summary>Create refuses a negative sort order with <c>ServerLocationErrors.SortOrderNegative</c>.</summary>
    [Fact]
    public void Create_NegativeSortOrder_Throws()
    {
        var ex = Assert.Throws<DomainRuleException>(() => ServerLocation.Create("DE", -1, true));

        Assert.Equal(ServerLocationErrors.SortOrderNegative, ex.Code);
    }

    /// <summary>Update refuses a negative sort order and leaves the entity untouched.</summary>
    [Fact]
    public void Update_NegativeSortOrder_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update("NL", -1, true));

        Assert.Equal(ServerLocationErrors.SortOrderNegative, ex.Code);
        Assert.Equal(2, entity.SortOrder);
        Assert.Equal("DE", entity.CountryCode);
    }

    /// <summary>SetSortOrder refuses a negative value and keeps the current order.</summary>
    [Fact]
    public void SetSortOrder_Negative_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.SetSortOrder(-1));

        Assert.Equal(ServerLocationErrors.SortOrderNegative, ex.Code);
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

    /// <summary>Create refuses a country code that is not exactly two letters.</summary>
    /// <param name="code">The refused country code.</param>
    [Theory]
    [InlineData("")]
    [InlineData("D")]
    [InlineData("DEU")]
    [InlineData("D1")]
    public void Create_InvalidCountryCode_Throws(string code)
    {
        var ex = Assert.Throws<DomainRuleException>(() => ServerLocation.Create(code, 0, true));

        Assert.Equal(ServerLocationErrors.CountryCodeInvalid, ex.Code);
    }

    /// <summary>Update refuses an invalid country code and keeps the stored one.</summary>
    [Fact]
    public void Update_InvalidCountryCode_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update("D1", 2, true));

        Assert.Equal(ServerLocationErrors.CountryCodeInvalid, ex.Code);
        Assert.Equal("DE", entity.CountryCode);
    }
}
