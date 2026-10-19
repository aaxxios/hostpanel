using Innovayse.Hostpanel.Domain.Faqs;
using Innovayse.Hostpanel.Domain.Faqs.Enums;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Faqs;

/// <summary>Unit tests for the <see cref="Faq"/> aggregate's behaviour.</summary>
public sealed class FaqTests
{
    /// <summary>Builds a question with the sample first-state values.</summary>
    /// <returns>A new, active question.</returns>
    private static Faq NewEntity() => Faq.Create(category: FaqCategory.General, sortOrder: 2, isActive: true);

    /// <summary>Create stores every field, starts with no translations and stamps the creation time.</summary>
    [Fact]
    public void Create_StoresFields_AndStartsWithoutTranslations()
    {
        var before = DateTimeOffset.UtcNow;

        var entity = NewEntity();

        Assert.Equal(FaqCategory.General, entity.Category);
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

        entity.Update(category: FaqCategory.Products, sortOrder: 7, isActive: false);

        Assert.Equal(FaqCategory.Products, entity.Category);
        Assert.Equal(7, entity.SortOrder);
        Assert.False(entity.IsActive);
        Assert.Equal(created, entity.CreatedAt);
    }

    /// <summary>Deactivate hides the question and Activate shows it again.</summary>
    [Fact]
    public void DeactivateThenActivate_TogglesVisibility()
    {
        var entity = NewEntity();

        entity.Deactivate();
        Assert.False(entity.IsActive);

        entity.Activate();
        Assert.True(entity.IsActive);
    }

    /// <summary>SetSortOrder moves the question in the display order and touches nothing else.</summary>
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

        entity.SetTranslation("en", question: "How?", answer: "Like this.");
        entity.SetTranslation("en", question: "How exactly?", answer: "Like that.");

        var translation = Assert.Single(entity.Translations);
        Assert.Equal("en", translation.Locale);
        Assert.Equal("How exactly?", translation.Question);
    }

    /// <summary>RemoveTranslation drops only the named locale and ignores one that is absent.</summary>
    [Fact]
    public void RemoveTranslation_DropsOnlyThatLocale()
    {
        var entity = NewEntity();
        entity.SetTranslation("en", question: "How?", answer: "Like this.");
        entity.SetTranslation("hy", question: "How exactly?", answer: "Like that.");

        entity.RemoveTranslation("en");
        entity.RemoveTranslation("ru");

        var remaining = Assert.Single(entity.Translations);
        Assert.Equal("hy", remaining.Locale);
    }

    /// <summary>Create refuses a negative sort order with <c>FaqErrors.SortOrderNegative</c>.</summary>
    [Fact]
    public void Create_NegativeSortOrder_Throws()
    {
        var ex = Assert.Throws<DomainRuleException>(() => Faq.Create(FaqCategory.General, -1, true));

        Assert.Equal(FaqErrors.SortOrderNegative, ex.Code);
    }

    /// <summary>Update refuses a negative sort order and leaves the entity untouched.</summary>
    [Fact]
    public void Update_NegativeSortOrder_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update(FaqCategory.General, -1, true));

        Assert.Equal(FaqErrors.SortOrderNegative, ex.Code);
        Assert.Equal(2, entity.SortOrder);
    }

    /// <summary>SetSortOrder refuses a negative value and keeps the current order.</summary>
    [Fact]
    public void SetSortOrder_Negative_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.SetSortOrder(-1));

        Assert.Equal(FaqErrors.SortOrderNegative, ex.Code);
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
