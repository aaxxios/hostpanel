using Innovayse.Hostpanel.Domain.Testimonials;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Testimonials;

/// <summary>Unit tests for the <see cref="Testimonial"/> aggregate's behaviour.</summary>
public sealed class TestimonialTests
{
    /// <summary>Builds a testimonial with the sample first-state values.</summary>
    /// <returns>A new, active testimonial.</returns>
    private static Testimonial NewEntity() => Testimonial.Create(name: "Ann", company: "Acme", avatarUrl: "/a.png", rating: 4, sortOrder: 2, isActive: true);

    /// <summary>Create stores every field, starts with no translations and stamps the creation time.</summary>
    [Fact]
    public void Create_StoresFields_AndStartsWithoutTranslations()
    {
        var before = DateTimeOffset.UtcNow;

        var entity = NewEntity();

        Assert.Equal("Ann", entity.Name);
        Assert.Equal("Acme", entity.Company);
        Assert.Equal("/a.png", entity.AvatarUrl);
        Assert.Equal(4, entity.Rating);
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

        entity.Update(name: "Bob", company: null, avatarUrl: null, rating: 5, sortOrder: 7, isActive: false);

        Assert.Equal("Bob", entity.Name);
        Assert.Null(entity.Company);
        Assert.Null(entity.AvatarUrl);
        Assert.Equal(5, entity.Rating);
        Assert.Equal(7, entity.SortOrder);
        Assert.False(entity.IsActive);
        Assert.Equal(created, entity.CreatedAt);
    }

    /// <summary>Deactivate hides the testimonial and Activate shows it again.</summary>
    [Fact]
    public void DeactivateThenActivate_TogglesVisibility()
    {
        var entity = NewEntity();

        entity.Deactivate();
        Assert.False(entity.IsActive);

        entity.Activate();
        Assert.True(entity.IsActive);
    }

    /// <summary>SetSortOrder moves the testimonial in the display order and touches nothing else.</summary>
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

        entity.SetTranslation("en", role: "CEO", quote: "Great.");
        entity.SetTranslation("en", role: null, quote: "Even better.");

        var translation = Assert.Single(entity.Translations);
        Assert.Equal("en", translation.Locale);
        Assert.Null(translation.Role);
    }

    /// <summary>RemoveTranslation drops only the named locale and ignores one that is absent.</summary>
    [Fact]
    public void RemoveTranslation_DropsOnlyThatLocale()
    {
        var entity = NewEntity();
        entity.SetTranslation("en", role: "CEO", quote: "Great.");
        entity.SetTranslation("hy", role: null, quote: "Even better.");

        entity.RemoveTranslation("en");
        entity.RemoveTranslation("ru");

        var remaining = Assert.Single(entity.Translations);
        Assert.Equal("hy", remaining.Locale);
    }

    /// <summary>Create refuses a negative sort order with <c>TestimonialErrors.SortOrderNegative</c>.</summary>
    [Fact]
    public void Create_NegativeSortOrder_Throws()
    {
        var ex = Assert.Throws<DomainRuleException>(() => Testimonial.Create("Ann", null, null, 3, -1, true));

        Assert.Equal(TestimonialErrors.SortOrderNegative, ex.Code);
    }

    /// <summary>Update refuses a negative sort order and leaves the entity untouched.</summary>
    [Fact]
    public void Update_NegativeSortOrder_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update("Ann", null, null, 3, -1, true));

        Assert.Equal(TestimonialErrors.SortOrderNegative, ex.Code);
        Assert.Equal(2, entity.SortOrder);
    }

    /// <summary>SetSortOrder refuses a negative value and keeps the current order.</summary>
    [Fact]
    public void SetSortOrder_Negative_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.SetSortOrder(-1));

        Assert.Equal(TestimonialErrors.SortOrderNegative, ex.Code);
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

    /// <summary>Create refuses an empty or whitespace name with <c>TestimonialErrors.NameRequired</c>.</summary>
    /// <param name="name">The invalid name.</param>
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_BlankName_Throws(string name)
    {
        var ex = Assert.Throws<DomainRuleException>(() => Testimonial.Create(name, null, null, 3, 0, true));

        Assert.Equal(TestimonialErrors.NameRequired, ex.Code);
    }

    /// <summary>Update refuses a blank name and keeps the current one.</summary>
    [Fact]
    public void Update_BlankName_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update("", null, null, 4, 2, true));

        Assert.Equal(TestimonialErrors.NameRequired, ex.Code);
        Assert.Equal("Ann", entity.Name);
    }

    /// <summary>Create refuses a rating outside 1 to 5 and accepts both ends.</summary>
    /// <param name="rating">The rating under test.</param>
    /// <param name="allowed">Whether the rating is accepted.</param>
    [Theory]
    [InlineData(0, false)]
    [InlineData(6, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    public void Create_Rating_IsHeldToOneThroughFive(int rating, bool allowed)
    {
        if (allowed)
        {
            Assert.Equal(rating, Testimonial.Create("Ann", null, null, rating, 0, true).Rating);
            return;
        }

        var ex = Assert.Throws<DomainRuleException>(() => Testimonial.Create("Ann", null, null, rating, 0, true));

        Assert.Equal(TestimonialErrors.RatingOutOfRange, ex.Code);
    }

    /// <summary>Update refuses an out-of-range rating and keeps the current one.</summary>
    [Fact]
    public void Update_RatingOutOfRange_Throws()
    {
        var entity = NewEntity();

        var ex = Assert.Throws<DomainRuleException>(() => entity.Update("Ann", null, null, 6, 2, true));

        Assert.Equal(TestimonialErrors.RatingOutOfRange, ex.Code);
        Assert.Equal(4, entity.Rating);
    }
}
