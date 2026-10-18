using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>POST /api/tickets</c>: staff can file a ticket on behalf of a client, and the action is
/// closed to a caller with no credential.
/// </summary>
/// <remarks>
/// Split out of the deleted <c>TicketsAdminEndpointTests</c>. Admin-only rather than every
/// <c>ResellerOrAdmin</c> caller -- <see cref="Innovayse.Hostpanel.API.Controllers.TicketsController.CreateAsync"/>
/// carries its own <c>[Authorize(Policy = Policies.Admin)]</c>.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AdminCreateTicketTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can create a ticket on behalf of a client.</summary>
    [Fact]
    public async Task AdminCanCreateATicketAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var departmentId = await SupportTestSeeds.CreateDepartmentAsync(client);

        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            clientId = 1,
            subject = "Admin-created ticket",
            message = "Filed by staff on behalf of the client.",
            departmentId,
            priority = nameof(TicketPriority.Medium),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>A missing department is refused 400 on the <c>departmentId</c> field with a specific message.</summary>
    [Fact]
    public async Task MissingDepartmentIsRefusedOnTheDepartmentFieldAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            clientId = 1,
            subject = "Admin-created ticket",
            message = "Filed by staff on behalf of the client.",
            departmentId = (int?)null,
            priority = nameof(TicketPriority.Medium),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("errors").GetProperty("departmentId")[0].GetString()
            .Should().Be("Select a department.");
    }

    /// <summary>A caller with no credential cannot file a ticket on behalf of a client.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/tickets", new
        {
            clientId = 1,
            subject = "Anything",
            message = "Anything.",
            departmentId = SupportTestSeeds.NoSuchId,
            priority = nameof(TicketPriority.Medium),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
