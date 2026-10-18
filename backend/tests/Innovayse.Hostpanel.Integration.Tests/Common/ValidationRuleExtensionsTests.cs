using FluentAssertions;
using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Common;

/// <summary>Pins the shared rule chains validators name instead of repeating.</summary>
public sealed class ValidationRuleExtensionsTests
{
    /// <summary>Largest page size the probe validator accepts.</summary>
    private const int MaxPageSize = 50;

    /// <summary>Message the probe reports for a bad id.</summary>
    private const string IdMessage = "id";

    /// <summary>Message the probe reports for a bad page.</summary>
    private const string PageMessage = "page";

    /// <summary>Message the probe reports for a bad page size.</summary>
    private const string PageSizeMessage = "size";

    /// <summary>A valid id, page and size pass.</summary>
    [Fact]
    public void ValidValues_Pass()
    {
        new ProbeValidator().Validate(new Probe(1, 1, MaxPageSize)).IsValid.Should().BeTrue();
    }

    /// <summary>An id of zero or less is refused with the supplied message.</summary>
    /// <param name="id">The id under test.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void NonPositiveId_IsRefused(int id)
    {
        var result = new ProbeValidator().Validate(new Probe(id, 1, 10));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(IdMessage);
    }

    /// <summary>A page below 1 is refused with the supplied message.</summary>
    [Fact]
    public void PageBelowOne_IsRefused()
    {
        var result = new ProbeValidator().Validate(new Probe(1, 0, 10));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(PageMessage);
    }

    /// <summary>A page size outside 1..max is refused with the supplied message.</summary>
    /// <param name="pageSize">The page size under test.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(MaxPageSize + 1)]
    public void PageSizeOutOfRange_IsRefused(int pageSize)
    {
        var result = new ProbeValidator().Validate(new Probe(1, 1, pageSize));

        result.Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be(PageSizeMessage);
    }

    /// <summary>The message the probe validator checks.</summary>
    /// <param name="Id">An identifier.</param>
    /// <param name="Page">A 1-based page.</param>
    /// <param name="PageSize">A page size.</param>
    private sealed record Probe(int Id, int Page, int PageSize);

    /// <summary>Applies each shared rule once.</summary>
    private sealed class ProbeValidator : AbstractValidator<Probe>
    {
        /// <summary>Initializes the validator and declares its rules.</summary>
        public ProbeValidator()
        {
            RuleFor(x => x.Id).MustBePositiveId(IdMessage);
            RuleFor(x => x.Page).MustBePage(PageMessage);
            RuleFor(x => x.PageSize).MustBePageSize(MaxPageSize, PageSizeMessage);
        }
    }
}
