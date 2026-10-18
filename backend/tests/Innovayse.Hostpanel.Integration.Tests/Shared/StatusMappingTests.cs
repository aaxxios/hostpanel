using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Controllers;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;
/// <summary>The one code → status table, pinned row by row so a stray substring cannot move a status.</summary>
public sealed class StatusMappingTests
{
    /// <summary>
    /// Each documented code answers the documented status: the table in <c>rules/controllers.md</c>,
    /// row by row, plus the product-neutral <c>HOSTPANEL_*</c> codes the shared layer emits itself.
    /// </summary>
    /// <param name="code">An error code.</param>
    /// <param name="status">The status the table lists for it.</param>
    [Theory]
    [InlineData(SharedErrorCodes.ValidationError, 400)]
    [InlineData("INVOICE_VALIDATION_FAILED", 422)]
    [InlineData("QUOTA_EXCEEDED", 402)]
    [InlineData(SharedErrorCodes.Unauthorized, 401)]
    [InlineData(SharedErrorCodes.Forbidden, 403)]
    [InlineData(HostpanelErrorCodes.INVOICE_NOT_FOUND, 404)]
    [InlineData(HostpanelErrorCodes.CLIENT_USER_NOT_LINKED, 404)]
    [InlineData("SKU_ALREADY_EXISTS", 409)]
    [InlineData("PAYLOAD_TOO_LARGE", 413)]
    [InlineData("RATE_LIMITED", 429)]
    [InlineData("ACCOUNT_LOCKED", 423)]
    [InlineData("GATEWAY_UPSTREAM_FAILED", 502)]
    [InlineData("UNEXPECTED", 500)]
    [InlineData(SharedErrorCodes.Unexpected, 500)]
    [InlineData(HostpanelErrorCodes.HOSTPANEL_RATE_LIMITED, 429)]
    [InlineData("SERVICE_UNAVAILABLE", 503)]
    [InlineData("GATEWAY_TIMEOUT", 503)]
    [InlineData("MAILER_NOT_CONFIGURED", 503)]
    [InlineData("VALIDATION_NOT_CONFIGURED", 422)]
    [InlineData("SHARE_NOT_FOUND_UPSTREAM_FAILED", 404)]
    [InlineData(HostpanelErrorCodes.CONTACT_SEND_FAILED, 502)]
    [InlineData(HostpanelErrorCodes.BRANDING_IMAGE_TOO_LARGE, 413)]
    [InlineData(HostpanelErrorCodes.SETUP_TOKEN_INVALID, 403)]
    [InlineData(SharedErrorCodes.DemoActionDisabled, 403)]
    [InlineData("SHARE_NOT_FOUND_SEND_FAILED", 404)]
    [InlineData("SOMETHING_ELSE", 400)]
    public void MapsCodeToStatus(string code, int status)
    {
        StatusMapping.For(code).Status.Should().Be(status);
    }
}
