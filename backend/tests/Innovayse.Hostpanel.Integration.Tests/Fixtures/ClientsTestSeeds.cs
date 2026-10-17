using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.IntegrationEvents.Contracts;
using Innovayse.Hostpanel.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Seeding and authentication helpers shared by every Clients use-case test file, extracted from
/// the endpoint-grouped files this A7 Task 5 batch split apart so the seeding logic exists exactly
/// once rather than once per use case.
/// </summary>
public static class ClientsTestSeeds
{
    /// <summary>Shared test password for every registered client user.</summary>
    public const string TestPassword = "Pass@123!";

    /// <summary>An id no seeded or created client ever takes.</summary>
    public const int NoSuchClientId = 999_999_999;

    /// <summary>An Identity subject no account, seeded or registered, ever takes.</summary>
    public const string NoSuchUserId = "no-such-subject";

    /// <summary>Builds a client carrying the seeded admin's bearer token.</summary>
    /// <param name="factory">The factory whose admin credentials to mint a token from.</param>
    /// <returns>An authenticated admin <see cref="HttpClient"/>.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Seeds a client row directly against the host's database, bypassing HTTP.</summary>
    /// <param name="factory">The factory whose database to seed against.</param>
    /// <param name="firstName">The seeded client's first name.</param>
    /// <param name="lastName">The seeded client's last name.</param>
    /// <returns>The new client's primary key and the Identity subject it is linked to.</returns>
    /// <remarks>
    /// The subject it links to resolves to nobody: a handler that asks the identity provider
    /// about it answers <c>IDENTITY_USER_NOT_FOUND</c>, which is why a refusal that needs a
    /// genuinely resolving subject goes through <see cref="RegisterRealClientAsync"/> instead.
    /// </remarks>
    public static async Task<(int ClientId, string UserId)> SeedClientAsync(
        IntegrationTestFactory factory, string firstName = "Ada", string lastName = "Lovelace")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var userId = $"user-{Guid.NewGuid():N}";
        var client = Client.Create(userId, firstName, lastName, $"{firstName.ToLowerInvariant()}-{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);
        await db.SaveChangesAsync(CancellationToken.None);
        return (client.Id, userId);
    }

    /// <summary>
    /// Creates a fresh Identity subject straight through <see cref="IUserService"/>, so no
    /// <see cref="ClientRegisteredIntegrationEvent"/> is ever published for it and no client row
    /// ever links to it.
    /// </summary>
    /// <param name="factory">The factory whose host owns the identity store.</param>
    /// <param name="label">Distinguishes the created address between callers in the same test.</param>
    /// <returns>The subject id and email of the newly created, still-unlinked account.</returns>
    /// <remarks>
    /// <c>/api/auth/register</c> publishes the event that makes <c>CreateClientOnRegisterHandler</c>
    /// link a client row, and that handler runs on a queue: a test that needs an unlinked subject
    /// would race it and sometimes find the subject already linked.
    /// </remarks>
    public static async Task<(string UserId, string Email)> CreateUnlinkedSubjectAsync(
        IntegrationTestFactory factory, string label)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserService>();
        var email = $"{label}-{Guid.NewGuid():N}@example.test";
        var userId = await users.CreateAsync(email, TestPassword, "Fresh", "Subject", CancellationToken.None);
        return (userId, email);
    }

    /// <summary>
    /// Registers a real Identity account through <c>/api/auth/register</c> and runs
    /// <c>CreateClientOnRegisterHandler</c> inline so the account is linked to a client row.
    /// </summary>
    /// <param name="factory">The factory to register against and resolve the bus from.</param>
    /// <param name="label">Distinguishes the registered address between callers in the same test.</param>
    /// <returns>The subject and email of the newly registered, now client-linked account.</returns>
    /// <remarks>
    /// <see cref="SeedClientAsync"/> links a client row to a subject nobody registered, which
    /// answers <c>IDENTITY_USER_NOT_FOUND</c> the moment a handler asks the identity provider
    /// about it -- this refusal needs a subject that genuinely resolves.
    /// </remarks>
    public static async Task<(string UserId, string Email)> RegisterRealClientAsync(
        IntegrationTestFactory factory, string label = "linked")
    {
        var http = factory.CreateClient();
        var email = $"{label}-{Guid.NewGuid():N}@example.test";
        var registered = await http.PostAsJsonAsync(
            "/api/auth/register", new { email, password = TestPassword, firstName = "Linked", lastName = "Person" });
        registered.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = (await registered.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetString()!;

        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        // Registration publishes this event to a local queue that the whole suite shares, so a
        // 10 s poll still timed out under a full run. Running the same real handler inline makes
        // the row exist now. The handler skips a subject that already has a client, and the
        // unique index on UserId refuses a second row if the queued copy wins the race.
        try
        {
            await bus.InvokeAsync(new ClientRegisteredIntegrationEvent(userId, email, "Linked", "Person"));
        }
        catch (DbUpdateException)
        {
            // The queued copy inserted the row between the handler's check and its save.
        }

        var db = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        (await db.Clients.AnyAsync(c => c.UserId == userId)).Should().BeTrue();
        return (userId, email);
    }

    /// <summary>
    /// Registers a client user over HTTP, logs in, and waits for the Wolverine handler that
    /// creates the corresponding <see cref="Client"/> row -- the client-portal equivalent of
    /// <see cref="SeedClientAsync"/>, for use cases resolved from the credential rather than a path id.
    /// </summary>
    /// <param name="factory">The factory to register and mint a token from.</param>
    /// <param name="label">Distinguishes the registered address between callers in the same test.</param>
    /// <returns>The authenticated client, its numeric client id and its Identity subject.</returns>
    public static async Task<(HttpClient Client, int ClientId, string UserId)> RegisterClientAsync(
        IntegrationTestFactory factory, string label)
    {
        var httpClient = factory.CreateClient();
        var email = $"clients-{label}-{Guid.NewGuid():N}@example.test";
        var registered = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Portal",
            lastName = label,
        });
        registered.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = (await registered.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetString()!;

        var token = await factory.GetClientTokenAsync(email, TestPassword);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var clientId = 0;
        for (var i = 0; i < 10 && clientId == 0; i++)
        {
            await Task.Delay(300);
            var me = await httpClient.GetAsync("/api/clients/me");
            if (me.IsSuccessStatusCode)
            {
                var json = await me.Content.ReadFromJsonAsync<JsonElement>();
                if (json.TryGetProperty("id", out var idProp))
                {
                    clientId = idProp.GetInt32();
                }
            }
        }

        clientId.Should().BeGreaterThan(0, "the Wolverine handler should have created the client record");
        return (httpClient, clientId, userId);
    }

    /// <summary>Seeds a pending invitation directly against the host's database.</summary>
    /// <param name="factory">The factory whose database to seed against.</param>
    /// <param name="clientId">The client the invitation is for.</param>
    /// <param name="email">The invitee's email address.</param>
    /// <returns>The invitation's unguessable acceptance token.</returns>
    public static async Task<string> SeedInvitationAsync(IntegrationTestFactory factory, int clientId, string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var invitation = Invitation.Create(clientId, email, "Invited", "Person", Domain.Clients.Enums.ClientPermission.ViewDomains);
        db.Invitations.Add(invitation);
        await db.SaveChangesAsync(CancellationToken.None);
        return invitation.Token;
    }

    /// <summary>Reads the <c>type</c> field of a refusal answered by <c>BaseApiController</c>.</summary>
    /// <param name="response">The refusal.</param>
    /// <returns>The error code.</returns>
    public static async Task<string?> CodeOfAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("type").GetString();
    }

    /// <summary>Asserts the refusal carries the expected error code.</summary>
    /// <param name="response">The refusal.</param>
    /// <param name="expected">The code it must carry.</param>
    /// <returns>A task that completes once the body has been read.</returns>
    public static async Task AssertCodeAsync(HttpResponseMessage response, string expected) =>
        (await CodeOfAsync(response)).Should().Be(expected);

    /// <summary>The body <c>PUT /api/clients/{id}</c> or <c>PUT /api/clients/me</c> takes, every field populated except the override.</summary>
    /// <param name="firstName">The first name to send.</param>
    /// <param name="currency">The currency to send, or null to leave it untouched.</param>
    /// <param name="status">The status string to send, or null to leave it untouched.</param>
    /// <param name="email">The email to send, or null to leave it untouched.</param>
    /// <returns>The request body.</returns>
    public static object UpdateBody(
        string firstName = "Ada", string? currency = null, string? status = null, string? email = null) => new
        {
            email,
            firstName,
            lastName = "Lovelace",
            companyName = (string?)null,
            phone = (string?)null,
            street = (string?)null,
            address2 = (string?)null,
            city = (string?)null,
            state = (string?)null,
            postCode = (string?)null,
            country = (string?)null,
            language = (string?)null,
            currency,
            paymentMethod = (string?)null,
            billingContact = (string?)null,
            adminNotes = (string?)null,
            notifyGeneral = true,
            notifyInvoice = true,
            notifySupport = true,
            notifyProduct = true,
            notifyDomain = true,
            notifyAffiliate = true,
            lateFees = true,
            overdueNotices = true,
            taxExempt = false,
            separateInvoices = false,
            disableCcProcessing = false,
            marketingOptIn = false,
            statusUpdate = true,
            allowSso = true,
            status,
        };

    /// <summary>The body <c>POST /api/clients/admin</c> takes, linking to an existing Identity subject.</summary>
    /// <param name="existingUserId">The subject id to link.</param>
    /// <param name="currency">The currency to send, or null to leave it unset.</param>
    /// <returns>The request body.</returns>
    public static object AdminCreateBody(string existingUserId, string? currency = null) => new
    {
        createNewUser = false,
        existingUserId,
        email = (string?)null,
        password = (string?)null,
        firstName = "Second",
        lastName = "Client",
        companyName = (string?)null,
        phone = (string?)null,
        street = (string?)null,
        address2 = (string?)null,
        city = (string?)null,
        state = (string?)null,
        postCode = (string?)null,
        country = (string?)null,
        currency,
        language = (string?)null,
        paymentMethod = (string?)null,
        billingContact = (string?)null,
        adminNotes = (string?)null,
        status = (string?)null,
    };

    /// <summary>A contact body with every field populated.</summary>
    /// <param name="firstName">The first name to send.</param>
    /// <returns>The request body.</returns>
    public static object ContactBody(string firstName) => new
    {
        firstName,
        lastName = "Probe",
        companyName = (string?)null,
        email = "probe@example.test",
        phone = (string?)null,
        type = "General",
        street = (string?)null,
        address2 = (string?)null,
        city = (string?)null,
        state = (string?)null,
        postCode = (string?)null,
        country = (string?)null,
        notifyGeneral = true,
        notifyInvoice = true,
        notifySupport = true,
        notifyProduct = true,
        notifyDomain = true,
        notifyAffiliate = true,
    };
}
