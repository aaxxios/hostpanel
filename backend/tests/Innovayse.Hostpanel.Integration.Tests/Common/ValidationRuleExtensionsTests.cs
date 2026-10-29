namespace Innovayse.Hostpanel.Integration.Tests.Common;

/// <summary>Pins the shared rule chains validators name instead of repeating.</summary>
public sealed class ValidationRuleExtensionsTests
{
    /// <summary>Largest page size the probe validator accepts.</summary>
    internal const int MaxPageSize = 50;

    /// <summary>Message the probe reports for a bad id.</summary>
    internal const string IdMessage = "id";

    /// <summary>Message the probe reports for a bad page.</summary>
    internal const string PageMessage = "page";

    /// <summary>Message the probe reports for a bad page size.</summary>
    internal const string PageSizeMessage = "size";

    /// <summary>A valid id, page and size pass.</summary>
    [Fact]
    public void ValidValues_Pass()
    {
        new PagingProbeValidator().Validate(new PagingProbe(1, 1, MaxPageSize)).IsValid.Should().BeTrue();
    }

    /// <summary>An id of zero or less is refused with the supplied message.</summary>
    /// <param name="id">The id under test.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void NonPositiveId_IsRefused(int id)
    {
        var result = new PagingProbeValidator().Validate(new PagingProbe(id, 1, 10));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(IdMessage);
    }

    /// <summary>A page below 1 is refused with the supplied message.</summary>
    [Fact]
    public void PageBelowOne_IsRefused()
    {
        var result = new PagingProbeValidator().Validate(new PagingProbe(1, 0, 10));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(PageMessage);
    }

    /// <summary>A page size outside 1..max is refused with the supplied message.</summary>
    /// <param name="pageSize">The page size under test.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(MaxPageSize + 1)]
    public void PageSizeOutOfRange_IsRefused(int pageSize)
    {
        var result = new PagingProbeValidator().Validate(new PagingProbe(1, 1, pageSize));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(PageSizeMessage);
    }
}
