using Innovayse.Hostpanel.Application.Common.Errors;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// Integration tests for <c>POST /api/me/services/{id}/change-password</c>: a caller may change
/// the hosting account password of their own provisioned service, a stranger's is refused.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ChangeMyServicePasswordTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Changing a stranger's service password is refused, not told which part is wrong.</summary>
    [Fact]
    public async Task ChangePassword_AStrangersService_Returns404WithServiceNotFound()
    {
        var http = factory.CreateClient();
        var adminToken = await factory.GetAdminTokenAsync();
        var ownerServiceId = await ProvisioningTestSeeds.SeedServiceAsync(factory, http, adminToken);
        (await http.PostAsync($"/api/provisioning/{ownerServiceId}/provision", null)).EnsureSuccessStatusCode();

        var strangerEmail = $"change-pwd-stranger-{Guid.NewGuid():N}@example.com";
        var registerResponse = await http.PostAsJsonAsync("/api/auth/register", new
        {
            email = strangerEmail,
            password = "Pass@123!",
            firstName = "Stranger",
            lastName = "User"
        });
        registerResponse.EnsureSuccessStatusCode();
        var strangerToken = await factory.GetClientTokenAsync(strangerEmail, "Pass@123!");
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", strangerToken);

        var response = await http.PostAsJsonAsync(
            $"/api/me/services/{ownerServiceId}/change-password", new { newPassword = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ProvisioningTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>Changing the password without authentication is refused.</summary>
    [Fact]
    public async Task ChangePassword_Returns401_WithoutAuthAsync()
    {
        var http = factory.CreateClient();

        var response = await http.PostAsJsonAsync(
            $"/api/me/services/{ProvisioningTestSeeds.NoSuchServiceId}/change-password",
            new { newPassword = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A non-positive id is refused by the validator with 400 <c>VALIDATION_ERROR</c> before the handler runs.</summary>
    [Fact]
    public async Task ChangePassword_WithNonPositiveId_Returns400ValidationErrorAsync()
    {
        var (client, _) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "changemyservicepassword-validation");

        var response = await client.PostAsJsonAsync(
            "/api/me/services/0/change-password",
            new { newPassword = "N3wPassw0rd!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(SharedErrorCodes.ValidationError);
    }
}
