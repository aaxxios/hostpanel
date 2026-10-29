using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Infrastructure.Pdf.Settings;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Pdf;

/// <summary><see cref="InvoicePdfOptionsValidator"/> refuses invoice PDF ceilings that would leave rendering unusable.</summary>
public sealed class InvoicePdfOptionsValidatorTests
{
    /// <summary>The validator under test.</summary>
    private readonly InvoicePdfOptionsValidator _validator = new();

    /// <summary>The defaults are usable: a host that sets nothing still renders invoices.</summary>
    [Fact]
    public void Defaults_Pass()
    {
        _validator.Validate(null, new InvoicePdfOptions()).Succeeded.Should().BeTrue();
    }

    /// <summary>A zero or negative line ceiling refuses startup and names the key.</summary>
    /// <param name="maxLines">The unusable ceiling.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ANonPositiveLineCeiling_Fails(int maxLines)
    {
        var result = _validator.Validate(null, new InvoicePdfOptions { MaxLines = maxLines });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(nameof(InvoicePdfOptions.MaxLines));
    }

    /// <summary>A zero or negative render limit refuses startup and names the key.</summary>
    /// <param name="maxConcurrentRenders">The unusable limit.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void ANonPositiveRenderLimit_Fails(int maxConcurrentRenders)
    {
        var result = _validator.Validate(null, new InvoicePdfOptions { MaxConcurrentRenders = maxConcurrentRenders });

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(nameof(InvoicePdfOptions.MaxConcurrentRenders));
    }
}
