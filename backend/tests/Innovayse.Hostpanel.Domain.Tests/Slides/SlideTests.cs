using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Domain.Slides.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Slides;

/// <summary>Unit tests for the <see cref="Slide"/> aggregate's translation and ordering behaviour.</summary>
public sealed class SlideTests
{
    /// <summary>A fixed moment the visibility tests measure against.</summary>
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Builds an active slide with no translations.</summary>
    /// <returns>A new slide at position 2.</returns>
    private static Slide NewEntity() => Slide.Create(Settings(isActive: true), Now);

    /// <summary>Builds the text of one locale with the given headline and bullet points.</summary>
    /// <param name="locale">The locale the text is written in.</param>
    /// <param name="title">The headline.</param>
    /// <param name="features">The bullet points, or null for none.</param>
    /// <returns>The translation content.</returns>
    private static SlideTranslationContent Content(string locale, string title, IReadOnlyList<string>? features) =>
        new(locale, title, null, null, features, null, null);

    /// <summary>
    /// ReplaceTranslations leaves exactly the given locales: a locale it does not name is removed, a
    /// kept one is updated in place and a new one is added with its bullet points.
    /// </summary>
    [Fact]
    public void ReplaceTranslations_RemovesUnlisted_UpdatesKept_AddsNew()
    {
        var entity = NewEntity();
        entity.ReplaceTranslations([Content("en", "Hosting", null), Content("ru", "Hosting Ru", null)]);

        entity.ReplaceTranslations([Content("en", "Fast hosting", ["SSL", "Backups"]), Content("hy", "Hosting Hy", null)]);

        Assert.Equal(["en", "hy"], entity.Translations.Select(t => t.Locale).Order());
        var en = entity.Translations.Single(t => t.Locale == "en");
        Assert.Equal("Fast hosting", en.Title);
        Assert.Equal(["SSL", "Backups"], en.Features!);
    }

    /// <summary>When a locale is sent twice, the later entry is the one kept, and only one row exists for it.</summary>
    [Fact]
    public void ReplaceTranslations_DuplicateLocale_LaterEntryWins()
    {
        var entity = NewEntity();

        entity.ReplaceTranslations([Content("en", "First", null), Content("en", "Second", null)]);

        var only = Assert.Single(entity.Translations);
        Assert.Equal("Second", only.Title);
    }

    /// <summary>SetSortOrder moves the slide to the new position.</summary>
    [Fact]
    public void SetSortOrder_MovesTheSlide()
    {
        var entity = NewEntity();

        entity.SetSortOrder(7);

        Assert.Equal(7, entity.SortOrder);
    }

    /// <summary>Create applies the settings and raises <see cref="Innovayse.Hostpanel.Domain.Slides.Events.SlideCreatedEvent"/>.</summary>
    [Fact]
    public void Create_AppliesTheSettingsAndRaisesTheEvent()
    {
        var entity = NewEntity();

        Assert.Equal("cloud-check", entity.IconName);
        Assert.Equal(2, entity.SortOrder);
        Assert.True(entity.IsActive);
        var raised = Assert.Single(entity.DomainEvents);
        Assert.Equal(Now, Assert.IsType<Innovayse.Hostpanel.Domain.Slides.Events.SlideCreatedEvent>(raised).OccurredAt);
    }

    /// <summary>Reconfigure replaces the settings as one edit and leaves the translations alone.</summary>
    [Fact]
    public void Reconfigure_ReplacesTheSettingsAndKeepsTranslations()
    {
        var entity = NewEntity();
        entity.ReplaceTranslations([Content("en", "Hosting", null)]);

        entity.Reconfigure(Settings(isActive: false) with { IconName = "server", ProductId = 5, TargetAudience = SlideAudience.Guest });

        Assert.Equal("server", entity.IconName);
        Assert.Equal(5, entity.ProductId);
        Assert.False(entity.IsActive);
        Assert.Equal(SlideAudience.Guest, entity.TargetAudience);
        Assert.Single(entity.Translations);
    }

    /// <summary>Activate and Deactivate toggle the active flag.</summary>
    [Fact]
    public void ActivateAndDeactivate_ToggleTheFlag()
    {
        var entity = Slide.Create(Settings(isActive: false), Now);

        entity.Activate();
        Assert.True(entity.IsActive);

        entity.Deactivate();
        Assert.False(entity.IsActive);
    }

    /// <summary>An inactive slide is never visible.</summary>
    [Fact]
    public void IsVisibleAt_IsFalseWhenInactive()
    {
        Assert.False(Slide.Create(Settings(isActive: false), Now).IsVisibleAt(Now));
    }

    /// <summary>An active slide is visible only inside its window; open ends do not limit it.</summary>
    /// <param name="fromOffsetDays">Start of the window relative to now, or null for open.</param>
    /// <param name="untilOffsetDays">End of the window relative to now, or null for open.</param>
    /// <param name="expected">Whether the slide is visible now.</param>
    [Theory]
    [InlineData(null, null, true)]
    [InlineData(-1, 1, true)]
    [InlineData(1, null, false)]
    [InlineData(null, -1, false)]
    public void IsVisibleAt_RespectsTheWindow(int? fromOffsetDays, int? untilOffsetDays, bool expected)
    {
        var entity = Slide.Create(Settings(isActive: true) with
        {
            VisibleFrom = fromOffsetDays is null ? null : Now.AddDays(fromOffsetDays.Value),
            VisibleUntil = untilOffsetDays is null ? null : Now.AddDays(untilOffsetDays.Value),
        }, Now);

        Assert.Equal(expected, entity.IsVisibleAt(Now));
    }

    /// <summary>Builds slide settings at position 2 with no visibility window.</summary>
    /// <param name="isActive">Whether the slide is active.</param>
    /// <returns>The settings.</returns>
    private static SlideSettings Settings(bool isActive) =>
        new("cloud-check", "#0ea5e9", "/slide.png", null, null, null, 2, isActive, SlideAudience.All, null, null);
}
