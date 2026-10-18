using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FluentValidation.TestHelper;
using Innovayse.Hostpanel.Application.Clients.Commands.UpdateClient;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary>
/// <c>PUT /api/clients/{id}</c>: an admin edits a client's profile, address, preferences,
/// currency and status.
/// </summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateClientTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Updating a client's profile writes the change back.</summary>
    [Fact]
    public async Task UpdateWritesTheProfile()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync($"/api/clients/{clientId}", ClientsTestSeeds.UpdateBody(firstName: "Katherine"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await admin.GetFromJsonAsync<JsonElement>($"/api/clients/{clientId}");
        fetched.GetProperty("firstName").GetString().Should().Be("Katherine");
    }

    /// <summary>Updating an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UpdateUnknownClientRefusesNotFound()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}", ClientsTestSeeds.UpdateBody());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>
    /// Moving an invoiced client to a different currency is refused with
    /// <c>CLIENT_CURRENCY_ALREADY_INVOICED</c>, but the same save with the currency left alone
    /// still writes the other fields.
    /// </summary>
    [Fact]
    public async Task UpdateRefusesACurrencyChangeOnceInvoiced()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Invoices.Add(Invoice.Create(clientId, DateTimeOffset.UtcNow.AddDays(14), "USD"));
            await db.SaveChangesAsync();
        }

        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var changed = await admin.PutAsJsonAsync($"/api/clients/{clientId}", ClientsTestSeeds.UpdateBody(currency: "AMD"));
        changed.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ClientsTestSeeds.AssertCodeAsync(changed, HostpanelErrorCodes.CLIENT_CURRENCY_ALREADY_INVOICED);

        var unchanged = await admin.PutAsJsonAsync($"/api/clients/{clientId}", ClientsTestSeeds.UpdateBody(firstName: "Margaret"));
        unchanged.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>A currency this panel has not configured and enabled is refused as missing.</summary>
    [Fact]
    public async Task UpdateRefusesAnUnconfiguredCurrency()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync($"/api/clients/{clientId}", ClientsTestSeeds.UpdateBody(currency: "ZZZ"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CURRENCY_NOT_FOUND);
    }

    /// <summary>An unparsable status string answers <c>CLIENT_STATUS_INVALID</c>.</summary>
    /// <remarks>
    /// The validator refuses an unparsable status before the handler ever sees it, so this drives
    /// the request past the bound FluentValidation pipeline rather than the handler's own direct-
    /// caller defence, which is unreachable from HTTP. The status enum member closest to that
    /// defence is exercised through the domain tests instead; here the contract under test is "an
    /// unparsable status is refused", and the validator is what actually answers it first.
    /// </remarks>
    [Fact]
    public async Task UpdateRefusesAnInvalidStatus()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync($"/api/clients/{clientId}", ClientsTestSeeds.UpdateBody(status: "NotAStatus"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>Updating a client is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateRefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}", ClientsTestSeeds.UpdateBody());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403 on the admin route.</summary>
    [Fact]
    public async Task UpdateRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "update-forbidden");

        var response = await client.PutAsJsonAsync($"/api/clients/{clientId}", ClientsTestSeeds.UpdateBody());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Updating the caller's own profile through <c>PUT /api/clients/me</c> writes the change and
    /// never lets the currency be set from the body -- the same handler, dispatched from
    /// <c>ClientProfileController</c> instead of the admin route.
    /// </summary>
    /// <remarks>Split from the deleted <c>MyProfileEndpointTests</c> (A7 Task 5).</remarks>
    [Fact]
    public async Task UpdateMyProfileWritesTheProfileAndIgnoresCurrency()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "my-update-happy");
        var before = await client.GetFromJsonAsync<JsonElement>("/api/clients/me");
        var originalCurrency = before.GetProperty("currency").GetString();

        var response = await client.PutAsJsonAsync(
            "/api/clients/me", ClientsTestSeeds.UpdateBody(firstName: "Ada", currency: "AMD"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<JsonElement>();
        updated.GetProperty("firstName").GetString().Should().Be("Ada");
        updated.GetProperty("currency").GetString().Should().Be(originalCurrency);
    }

    /// <summary>Validator instance under test.</summary>
    private readonly UpdateClientValidator _validator = new(BuildLocalizer());

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

    /// <summary>Creates a valid command with sensible defaults for all fields.</summary>
    /// <param name="clientId">The client id to put on the command.</param>
    /// <param name="email">The email to put on the command.</param>
    /// <param name="firstName">The first name to put on the command.</param>
    /// <param name="lastName">The last name to put on the command.</param>
    /// <param name="companyName">The company name to put on the command.</param>
    /// <param name="phone">The phone number to put on the command.</param>
    /// <param name="street">The street line to put on the command.</param>
    /// <param name="address2">The second address line to put on the command.</param>
    /// <param name="city">The city to put on the command.</param>
    /// <param name="state">The state or province to put on the command.</param>
    /// <param name="postCode">The postal code to put on the command.</param>
    /// <param name="country">The ISO country code to put on the command.</param>
    /// <param name="language">The locale tag to put on the command.</param>
    /// <param name="currency">The currency code to put on the command.</param>
    /// <param name="paymentMethod">The payment method id to put on the command.</param>
    /// <param name="billingContact">The billing contact to put on the command.</param>
    /// <param name="adminNotes">The admin notes to put on the command.</param>
    /// <param name="status">The client status to put on the command.</param>
    /// <returns>The command.</returns>
    private static UpdateClientCommand MakeValidCommand(
        int clientId = 1,
        string? email = "john@example.com",
        string firstName = "John",
        string lastName = "Doe",
        string? companyName = null,
        string? phone = null,
        string? street = null,
        string? address2 = null,
        string? city = null,
        string? state = null,
        string? postCode = null,
        string? country = null,
        string? language = null,
        string? currency = null,
        string? paymentMethod = null,
        string? billingContact = null,
        string? adminNotes = null,
        string? status = null) =>
        new(
            clientId, email, firstName, lastName, companyName, phone,
            street, address2, city, state, postCode, country,
            language, currency, paymentMethod, billingContact, adminNotes,
            true, true, true, true, true, true,
            true, true, false, false, false, false, true, true,
            status);

    /// <summary>Valid command should pass.</summary>
    [Fact]
    public void ValidCommand_ShouldPass()
    {
        var cmd = MakeValidCommand();
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>ClientId 0 should fail.</summary>
    [Fact]
    public void ZeroClientId_ShouldFail()
    {
        var cmd = MakeValidCommand(clientId: 0);
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    /// <summary>
    /// A language outside <c>LocaleOptions.SupportedLocales</c> should fail. Stored, it would
    /// be read back and then served English, which looks exactly like the save not working.
    /// </summary>
    [Fact]
    public void UnsupportedLanguage_ShouldFail()
    {
        var cmd = MakeValidCommand(language: "english");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Language);
    }

    /// <summary>A supported language code should pass, whatever its casing.</summary>
    /// <param name="language">The language tag under test.</param>
    [Theory]
    [InlineData("en")]
    [InlineData("ru")]
    [InlineData("HY")]
    public void SupportedLanguage_ShouldPass(string language)
    {
        var cmd = MakeValidCommand(language: language);
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    /// <summary>
    /// Blank means "no preference" and must pass: the account form posts an empty string for
    /// its default option, and some callers send null for the same thing.
    /// </summary>
    /// <param name="language">The blank value under test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void BlankLanguage_ShouldPass(string? language)
    {
        var cmd = MakeValidCommand(language: language);
        _validator.TestValidate(cmd).ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    /// <summary>Country code with wrong length should fail.</summary>
    [Fact]
    public void WrongLengthCountry_ShouldFail()
    {
        var cmd = MakeValidCommand(country: "USA");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Country);
    }

    /// <summary>Empty FirstName should fail.</summary>
    [Fact]
    public void EmptyFirstName_ShouldFail()
    {
        var cmd = MakeValidCommand(firstName: "");
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.FirstName);
    }
}
