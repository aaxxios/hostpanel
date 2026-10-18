using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Innovayse.Hostpanel.Application.Clients.Commands.CreateClient;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary>
/// <c>POST /api/clients</c>: an admin links a new client record to an existing Identity subject.
/// </summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateClientTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating a client, then reading it back, round-trips its profile.</summary>
    [Fact]
    public async Task CreateThenGetRoundTripsTheClient()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);
        var userId = $"user-{Guid.NewGuid():N}";

        var created = await admin.PostAsJsonAsync("/api/clients", new
        {
            subjectUserId = userId,
            email = $"{userId}@example.test",
            firstName = "Grace",
            lastName = "Hopper",
            companyName = (string?)null,
        });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var clientId = await created.Content.ReadFromJsonAsync<int>();

        var fetched = await admin.GetAsync($"/api/clients/{clientId}");
        fetched.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("firstName").GetString().Should().Be("Grace");
    }

    /// <summary>Creating a client is not for anonymous callers.</summary>
    [Fact]
    public async Task CreateRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/clients", new
        {
            subjectUserId = $"user-{Guid.NewGuid():N}",
            email = "nobody@example.test",
            firstName = "Nobody",
            lastName = "Home",
            companyName = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403.</summary>
    [Fact]
    public async Task CreateRefusesAClient()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "create-forbidden");

        var response = await client.PostAsJsonAsync("/api/clients", new
        {
            subjectUserId = $"user-{Guid.NewGuid():N}",
            email = "nobody@example.test",
            firstName = "Nobody",
            lastName = "Home",
            companyName = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Validator instance under test.</summary>
    private readonly CreateClientValidator _validator = new(BuildLocalizer());

    /// <summary>
    /// Builds a localizer that answers every key with the key itself. These tests do not depend
    /// on the .resx sentences -- only on which rule fired -- so a stand-in that echoes the key is
    /// enough, and needs no resource files loaded.
    /// </summary>
    /// <returns>The localizer.</returns>
    private static IStringLocalizer<ApplicationValidationMessages> BuildLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<ApplicationValidationMessages>>();
        localizer.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        return localizer.Object;
    }

    /// <summary>Valid command should pass.</summary>
    [Fact]
    public void ValidCommand_ShouldPass()
    {
        var cmd = new CreateClientCommand("user-1", "john@example.com", "John", "Doe");
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>Empty SubjectUserId should fail.</summary>
    [Fact]
    public void EmptySubjectUserId_ShouldFail()
    {
        var cmd = new CreateClientCommand("", "john@example.com", "John", "Doe");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.SubjectUserId);
    }

    /// <summary>Invalid email should fail.</summary>
    [Fact]
    public void InvalidEmail_ShouldFail()
    {
        var cmd = new CreateClientCommand("user-1", "not-an-email", "John", "Doe");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>Empty FirstName should fail.</summary>
    [Fact]
    public void EmptyFirstName_ShouldFail()
    {
        var cmd = new CreateClientCommand("user-1", "john@example.com", "", "Doe");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.FirstName);
    }
}
