using Innovayse.Hostpanel.Domain.Projects;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Projects;

/// <summary>Unit tests for the <see cref="Project"/> aggregate's behaviour.</summary>
public sealed class ProjectTests
{
    /// <summary>Builds a project with the sample first-state values.</summary>
    /// <returns>A new, active project.</returns>
    private static Project NewEntity() => Project.Create(slug: "acme-site", url: "https://acme.example", imageUrls: new[] { "/a.png", "/b.png" }, sortOrder: 2, isActive: true);

    /// <summary>Create stores every field, starts with no translations and stamps the creation time.</summary>
    [Fact]
    public void Create_StoresFields_AndStartsWithoutTranslations()
    {
        var before = DateTimeOffset.UtcNow;

        var entity = NewEntity();

        Assert.Equal("acme-site", entity.Slug);
        Assert.Equal("https://acme.example", entity.Url);
        Assert.Equal(new[] { "/a.png", "/b.png" }, entity.ImageUrls);
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

        entity.Update(slug: "acme-portal", url: null, imageUrls: new[] { "/c.png" }, sortOrder: 7, isActive: false);

        Assert.Equal("acme-portal", entity.Slug);
        Assert.Null(entity.Url);
        Assert.Equal(new[] { "/c.png" }, entity.ImageUrls);
        Assert.Equal(7, entity.SortOrder);
        Assert.False(entity.IsActive);
        Assert.Equal(created, entity.CreatedAt);
    }

    /// <summary>Deactivate hides the project and Activate shows it again.</summary>
    [Fact]
    public void DeactivateThenActivate_TogglesVisibility()
    {
        var entity = NewEntity();

        entity.Deactivate();
        Assert.False(entity.IsActive);

        entity.Activate();
        Assert.True(entity.IsActive);
    }

    /// <summary>SetSortOrder moves the project in the display order and touches nothing else.</summary>
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

        entity.SetTranslation("en", title: "Acme", summary: "Short", description: null);
        entity.SetTranslation("en", title: "Acme Portal", summary: null, description: "Long");

        var translation = Assert.Single(entity.Translations);
        Assert.Equal("en", translation.Locale);
        Assert.Equal("Acme Portal", translation.Title);
    }

    /// <summary>RemoveTranslation drops only the named locale and ignores one that is absent.</summary>
    [Fact]
    public void RemoveTranslation_DropsOnlyThatLocale()
    {
        var entity = NewEntity();
        entity.SetTranslation("en", title: "Acme", summary: "Short", description: null);
        entity.SetTranslation("hy", title: "Acme Portal", summary: null, description: "Long");

        entity.RemoveTranslation("en");
        entity.RemoveTranslation("ru");

        var remaining = Assert.Single(entity.Translations);
        Assert.Equal("hy", remaining.Locale);
    }

    /// <summary>Create refuses a negative sort order with <c>ProjectErrors.SortOrderNegative</c>.</summary>
    [Fact]
    public void Create_NegativeSortOrder_Throws()
    {
        var ex = Assert.Throws<DomainRuleException>(() => Project.Create("acme", null, [], -1, true));

        Assert.Equal(ProjectErrors.SortOrderNegative, ex.Code);
    }

    /// <summary>Update refuses a negative sort order and leaves the entity untouched.</summary>
    [Fact]
    public void Update_NegativeSortOrder_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update("acme", null, [], -1, true));

        Assert.Equal(ProjectErrors.SortOrderNegative, ex.Code);
        Assert.Equal(2, entity.SortOrder);
    }

    /// <summary>SetSortOrder refuses a negative value and keeps the current order.</summary>
    [Fact]
    public void SetSortOrder_Negative_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.SetSortOrder(-1));

        Assert.Equal(ProjectErrors.SortOrderNegative, ex.Code);
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

    /// <summary>Create refuses an empty or whitespace slug with <c>ProjectErrors.SlugRequired</c>.</summary>
    /// <param name="slug">The invalid slug.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankSlug_Throws(string slug)
    {
        var ex = Assert.Throws<DomainRuleException>(() => Project.Create(slug, null, [], 0, true));

        Assert.Equal(ProjectErrors.SlugRequired, ex.Code);
    }

    /// <summary>Update refuses a blank slug and keeps the current one.</summary>
    [Fact]
    public void Update_BlankSlug_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update(" ", null, [], 2, true));

        Assert.Equal(ProjectErrors.SlugRequired, ex.Code);
        Assert.Equal("acme-site", entity.Slug);
    }
}
