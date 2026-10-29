using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Tests for <see cref="RegistrarResultExtensions.ToFailureCode"/>: the registrar's own reason
/// reaches the caller, a plain refusal or no reason falls back to the operation's code.
/// </summary>
public sealed class RegistrarResultExtensionsTests
{
    /// <summary>The operation code a handler passes in these tests.</summary>
    private const string OperationCode = HostpanelErrorCodes.NAMESERVERS_UPSTREAM_FAILED;

    /// <summary>Every specific registrar reason is answered as itself.</summary>
    /// <param name="registrarCode">The reason the registrar adapter gave.</param>
    [Theory]
    [InlineData(HostpanelErrorCodes.REGISTRAR_NOT_CONFIGURED)]
    [InlineData(HostpanelErrorCodes.REGISTRAR_DOMAIN_NOT_FOUND)]
    [InlineData(HostpanelErrorCodes.REGISTRAR_OPERATION_UNSUPPORTED)]
    [InlineData(HostpanelErrorCodes.REGISTRAR_DOMAIN_NOT_ACTIVE)]
    [InlineData(HostpanelErrorCodes.REGISTRAR_WHOIS_PRIVACY_UNAVAILABLE)]
    [InlineData(HostpanelErrorCodes.REGISTRAR_DOMAIN_UNAVAILABLE)]
    [InlineData(HostpanelErrorCodes.REGISTRAR_INSUFFICIENT_FUNDS)]
    [InlineData(HostpanelErrorCodes.REGISTRAR_CONTACT_VALIDATION_FAILED)]
    public void ToFailureCode_SpecificReason_ReturnsTheRegistrarCode(string registrarCode)
    {
        var result = new RegistrarResult(false, null, null, registrarCode);

        result.ToFailureCode(OperationCode).Should().Be(registrarCode);
    }

    /// <summary>A plain refusal says no more than the operation code, so the operation code wins.</summary>
    [Fact]
    public void ToFailureCode_PlainRefusal_ReturnsTheOperationCode()
    {
        var result = new RegistrarResult(false, null, null, HostpanelErrorCodes.REGISTRAR_ORDER_UPSTREAM_FAILED);

        result.ToFailureCode(OperationCode).Should().Be(OperationCode);
    }

    /// <summary>An adapter that gave no code falls back to the operation code.</summary>
    [Fact]
    public void ToFailureCode_NoCode_ReturnsTheOperationCode()
    {
        var result = new RegistrarResult(false, null, null, null);

        result.ToFailureCode(OperationCode).Should().Be(OperationCode);
    }
}
