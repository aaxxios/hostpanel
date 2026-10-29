using Innovayse.Hostpanel.Application.Notifications.Rendering;

namespace Innovayse.Hostpanel.Integration.Tests.Notifications;

/// <summary>
/// Tests for <see cref="BrandPalette"/>. The expected values are the same ones
/// <c>client/utils/brandPalette.test.ts</c> asserts, so the two ports cannot drift apart
/// without a red build on at least one side.
/// </summary>
/// <remarks>A pure function: no mocks and no database.</remarks>
public sealed class BrandPaletteTests
{
    /// <summary>The computed shade matches the TypeScript port's reference table.</summary>
    /// <param name="hex">The picked colour.</param>
    /// <param name="step">The Tailwind-style step requested.</param>
    /// <param name="expected">The shade the reference table names for that step.</param>
    [Theory]
    [InlineData("#0ea5e9", 500, "#0ea5e9")]
    [InlineData("#0ea5e9", 50, "#f5fbff")]
    [InlineData("#0ea5e9", 700, "#145e84")]
    [InlineData("#0ea5e9", 950, "#000d19")]
    [InlineData("#1a73e8", 700, "#154485")]
    [InlineData("#ffd600", 700, "#8d7710")]
    [InlineData("#a855f7", 200, "#e6d5ff")]
    public void Shade_MatchesTheTypeScriptPort(string hex, int step, string expected)
    {
        Assert.Equal(expected, BrandPalette.Shade(hex, step));
    }

    /// <summary>Step 500 is always the picked colour itself, regardless of hex case.</summary>
    [Fact]
    public void Shade_PinsThePickedColourAt500_WhateverTheCase()
    {
        Assert.Equal("#1a73e8", BrandPalette.Shade("#1A73E8", 500));
    }

    /// <summary>A colour that does not parse and a step the table does not name are both refused.</summary>
    [Fact]
    public void Shade_RefusesANonColourAndAnUnknownStep()
    {
        Assert.Throws<ArgumentException>(() => BrandPalette.Shade("blue", 500));
        Assert.Throws<ArgumentException>(() => BrandPalette.Shade("#0ea5e9", 550));
    }

    /// <summary>The contrast ratio matches the WCAG reference numbers.</summary>
    [Fact]
    public void ContrastRatio_MatchesTheWcagReferenceNumbers()
    {
        Assert.Equal(21, BrandPalette.ContrastRatio("#ffffff", "#000000"), tolerance: 0.001);
        Assert.Equal(2.78, BrandPalette.ContrastRatio("#ffffff", "#0ea5e9"), tolerance: 0.05);
        Assert.Equal(4.5, BrandPalette.ContrastRatio("#ffffff", "#1a73e8"), tolerance: 0.5);
        Assert.True(double.IsNaN(BrandPalette.ContrastRatio("blue", "#ffffff")));
    }

    /// <summary>The best-contrast text colour picks white on blues and ink on yellows.</summary>
    /// <param name="surface">The surface colour text is placed on.</param>
    /// <param name="expected">The text colour that gives the better contrast.</param>
    [Theory]
    [InlineData("#1a73e8", BrandPalette.White)]
    [InlineData("#a855f7", BrandPalette.White)]
    [InlineData("#ffd600", BrandPalette.Ink)]
    [InlineData("#7de3ff", BrandPalette.Ink)]
    public void BestTextOn_PutsWhiteOnBluesAndInkOnYellows(string surface, string expected)
    {
        Assert.Equal(expected, BrandPalette.BestTextOn(surface));
    }
}
