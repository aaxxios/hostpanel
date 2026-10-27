using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap.Settings;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Integrations.Namecheap;
/// <summary>
/// Unit tests for <see cref="NamecheapRegistrarProvider"/>'s previously-stubbed operations —
/// contact updates, email forwarding, and the DNS-management toggle — verifying both the
/// XML response parsing and the exact query parameters sent to the Namecheap API.
/// </summary>
public sealed class NamecheapRegistrarProviderTests : IAsyncLifetime
{
    /// <summary>The loopback server standing in for the Namecheap API; started per test class instance.</summary>
    private StubHttpServer namecheap = null!;

    /// <summary>Starts the stub Namecheap server before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        namecheap = await StubHttpServer.StartAsync();
    }

    /// <summary>Stops the stub Namecheap server after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        await namecheap.DisposeAsync();
    }

    /// <summary>Gets settings whose API URL points at the stub server.</summary>
    private NamecheapOptions Settings => new()
    {
        ApiUser = "testuser",
        ApiKey = "testkey",
        ClientIp = "127.0.0.1",
        ApiUrl = new Uri(namecheap.BaseAddress, "xml.response").ToString(),
    };

    /// <summary>
    /// Builds a provider aimed at the stub server, which answers <paramref name="responseXml"/> over
    /// real HTTP; the requests it received are read back from <see cref="namecheap"/>.
    /// </summary>
    /// <param name="responseXml">The XML body Namecheap answers with.</param>
    /// <returns>The provider under test.</returns>
    private NamecheapRegistrarProvider BuildProvider(string responseXml)
    {
        namecheap.Enqueue(HttpStatusCode.OK, responseXml, "text/xml");
        return new NamecheapRegistrarProvider(new NamecheapClient(new HttpClient(), Options.Create(Settings)));
    }

    /// <summary>Gets the full URL of the most recent request the stub received.</summary>
    /// <returns>The absolute URL, or an empty string when nothing was requested.</returns>
    private string LastRequestUrl()
    {
        var last = namecheap.Requests.LastOrDefault();
        return last is null ? string.Empty : new Uri(namecheap.BaseAddress, last.PathAndQuery.TrimStart('/')).ToString();
    }

    /// <summary>
    /// Extracts a single query parameter's decoded value from a request URL.
    /// Avoids depending on System.Web/AspNetCore query-parsing helpers just for tests.
    /// </summary>
    /// <param name="url">The request URL.</param>
    /// <param name="name">Name of the query parameter.</param>
    /// <returns>The decoded value of the parameter.</returns>
    private static string QueryParam(string url, string name)
    {
        var query = new Uri(url).Query.TrimStart('?');
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == name)
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return string.Empty;
    }

    /// <summary>Verifies: Modify Contact Details Async - Sends Set Contacts Command - For All Four Roles.</summary>
    /// <summary>Modify contact details async, sends set contacts command, for all four roles.</summary>
    [Fact]
    public async Task ModifyContactDetailsAsync_SendsSetContactsCommand_ForAllFourRolesAsync()
    {
        const string response = """<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse><DomainSetContactResult Domain="example.com" IsSuccess="true" /></CommandResponse></ApiResponse>""";
        var provider = BuildProvider(response);

        var contact = new DomainContact(
            "Jane", "Doe", "Acme Inc", "jane@example.com", "+1.5551234567",
            "123 Main St", null, "Springfield", "IL", "62701", "US");

        var result = await provider.ModifyContactDetailsAsync("example.com", contact, CancellationToken.None);

        result.Success.Should().BeTrue();
        var url = LastRequestUrl();
        QueryParam(url, "Command").Should().Be("namecheap.domains.setContacts");
        QueryParam(url, "RegistrantFirstName").Should().Be("Jane");
        QueryParam(url, "TechEmailAddress").Should().Be("jane@example.com");
        QueryParam(url, "AdminCountry").Should().Be("US");
        QueryParam(url, "AuxBillingLastName").Should().Be("Doe");
    }

    /// <summary>Verifies: Get Contact Details Async - Maps The Registrant Block Of The Get Contacts Response.</summary>
    [Fact]
    public async Task GetContactDetailsAsync_MapsTheRegistrantBlockAsync()
    {
        const string response = """<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse><DomainContactsResult Domain="example.com"><Registrant><FirstName>Jane</FirstName><LastName>Doe</LastName><OrganizationName></OrganizationName><EmailAddress>jane@example.com</EmailAddress><Phone>+1.5551234567</Phone><Address1>123 Main St</Address1><Address2 /><City>Springfield</City><StateProvince>IL</StateProvince><PostalCode>62701</PostalCode><Country>US</Country></Registrant><Tech><FirstName>Other</FirstName></Tech></DomainContactsResult></CommandResponse></ApiResponse>""";
        var provider = BuildProvider(response);

        var contact = await provider.GetContactDetailsAsync("example.com", CancellationToken.None);

        QueryParam(LastRequestUrl(), "Command").Should().Be("namecheap.domains.getContacts");
        contact.Should().Be(new DomainContact(
            "Jane", "Doe", null, "jane@example.com", "+1.5551234567",
            "123 Main St", null, "Springfield", "IL", "62701", "US"));
    }

    /// <summary>Verifies: Get Contact Details Async - Answers Null When The Response Has No Registrant.</summary>
    [Fact]
    public async Task GetContactDetailsAsync_WithoutRegistrantBlock_ReturnsNullAsync()
    {
        const string response = """<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse /></ApiResponse>""";
        var provider = BuildProvider(response);

        var contact = await provider.GetContactDetailsAsync("example.com", CancellationToken.None);

        contact.Should().BeNull();
    }

    /// <summary>Set Email Forwarding Async, when Disabling, clears All Forwards.</summary>
    /// <summary>Verifies: Set Email Forwarding Async - When Disabling - Clears All Forwards.</summary>
    [Fact]
    public async Task SetEmailForwardingAsync_WhenDisabling_ClearsAllForwardsAsync()
    {
        const string response = """<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse /></ApiResponse>""";
        var provider = BuildProvider(response);

        var result = await provider.SetEmailForwardingAsync("example.com", enabled: false, CancellationToken.None);

        result.Success.Should().BeTrue();
        QueryParam(LastRequestUrl(), "Command").Should().Be("namecheap.domains.dns.setEmailForwarding");
        QueryParam(LastRequestUrl(), "MailBox1").Should().BeEmpty();
    }

    /// <summary>Verifies: Add Email Forwarding Rule Async - Merges With Existing Forwards.</summary>
    [Fact]
    public async Task AddEmailForwardingRuleAsync_MergesWithExistingForwardsAsync()
    {
        // First call (GetEmailForwardsAsync inside AddEmailForwardingRuleAsync) returns one
        // existing forward; the provider must resend it alongside the newly added rule.
        const string getResponse = """
            <?xml version="1.0"?>
            <ApiResponse Status="OK">
              <CommandResponse>
                <DomainDNSGetEmailForwardingResult>
                  <Forward mailbox="sales">sales@destination.com</Forward>
                </DomainDNSGetEmailForwardingResult>
              </CommandResponse>
            </ApiResponse>
            """;

        var provider = BuildProvider(getResponse);

        var result = await provider.AddEmailForwardingRuleAsync(
            "example.com", "support", "support@destination.com", CancellationToken.None);

        result.Success.Should().BeTrue();
        namecheap.Requests.Should().HaveCount(2);

        var setUrl = new Uri(namecheap.BaseAddress, namecheap.Requests[1].PathAndQuery.TrimStart('/')).ToString();
        QueryParam(setUrl, "Command").Should().Be("namecheap.domains.dns.setEmailForwarding");

        var mailboxes = new[] { QueryParam(setUrl, "MailBox1"), QueryParam(setUrl, "MailBox2") };
        mailboxes.Should().Contain("sales").And.Contain("support");
    }

    /// <summary>Verifies: Set Dns Management Async - When Enabling - Calls Set Default.</summary>
    [Fact]
    public async Task SetDnsManagementAsync_WhenEnabling_CallsSetDefaultAsync()
    {
        const string response = """<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse /></ApiResponse>""";
        var provider = BuildProvider(response);

        var result = await provider.SetDnsManagementAsync("example.com", enabled: true, CancellationToken.None);

        result.Success.Should().BeTrue();
        /// <summary>Set Dns Management Async, when Disabling, makes No Api Call.</summary>
        QueryParam(LastRequestUrl(), "Command").Should().Be("namecheap.domains.dns.setDefault");
    }

    /// <summary>Verifies: Set Dns Management Async - When Disabling - Makes No Api Call.</summary>
    [Fact]
    public async Task SetDnsManagementAsync_WhenDisabling_MakesNoApiCallAsync()
    {
        var provider = BuildProvider("""<ApiResponse Status="OK"/>""");

        var result = await provider.SetDnsManagementAsync("example.com", enabled: false, CancellationToken.None);

        result.Success.Should().BeTrue();
        namecheap.Requests.Should().BeEmpty();
    }

    /// <summary>An unconfigured integration refuses a renewal and calls nothing.</summary>
    [Fact]
    public async Task RenewAsync_WhenNotConfigured_FailsWithoutCallingNamecheapAsync()
    {
        var unconfigured = Settings;
        unconfigured.ApiKey = string.Empty;
        var provider = new NamecheapRegistrarProvider(new NamecheapClient(new HttpClient(), Options.Create(unconfigured)));

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.com", "example.com", 1), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
        namecheap.Requests.Should().BeEmpty();
    }

    /// <summary>A renewal answer with an expiry reports that expiry.</summary>
    [Fact]
    public async Task RenewAsync_WithExpireDate_ReportsItAsync()
    {
        const string response = """<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse><DomainRenewResult DomainName="example.com" Renew="true" ExpireDate="2029-03-15" /></CommandResponse></ApiResponse>""";
        var provider = BuildProvider(response);

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.com", "example.com", 1), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ExpiresAt!.Value.Date.Should().Be(new DateTime(2029, 3, 15));
        QueryParam(LastRequestUrl(), "Command").Should().Be("namecheap.domains.renew");
    }

    /// <summary>
    /// A renewal answer without an expiry reports none, so the caller extends the current one;
    /// now + years would cost an early renewal its remaining days.
    /// </summary>
    [Fact]
    public async Task RenewAsync_WithoutExpireDate_ReportsNoExpiryAsync()
    {
        const string response = """<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse><DomainRenewResult DomainName="example.com" Renew="true" /></CommandResponse></ApiResponse>""";
        var provider = BuildProvider(response);

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.com", "example.com", 1), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ExpiresAt.Should().BeNull();
    }

    /// <summary>An OK envelope whose renew result is not <c>Renew="true"</c> renewed nothing.</summary>
    /// <param name="response">The Namecheap answer.</param>
    [Theory]
    [InlineData("""<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse><DomainRenewResult DomainName="example.com" Renew="false" /></CommandResponse></ApiResponse>""")]
    [InlineData("""<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse /></ApiResponse>""")]
    public async Task RenewAsync_WithoutRenewConfirmation_FailsAsync(string response)
    {
        var provider = BuildProvider(response);

        var result = await provider.RenewAsync(new DomainRenewalOrder("example.com", "example.com", 1), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("did not confirm");
    }

    /// <summary>The answer Namecheap gives a command it refuses.</summary>
    private const string ErrorResponse = """<?xml version="1.0"?><ApiResponse Status="ERROR"><Errors><Error Number="2033409">Domain is not available</Error></Errors></ApiResponse>""";

    /// <summary>Builds a provider whose settings have no API key.</summary>
    /// <returns>The unconfigured provider.</returns>
    private NamecheapRegistrarProvider BuildUnconfiguredProvider()
    {
        var unconfigured = Settings;
        unconfigured.ApiKey = string.Empty;
        return new NamecheapRegistrarProvider(new NamecheapClient(new HttpClient(), Options.Create(unconfigured)));
    }

    /// <summary>A registration on an unconfigured integration is refused, so a paid order is refunded.</summary>
    [Fact]
    public async Task RegisterAsync_WhenNotConfigured_FailsAsync()
    {
        var result = await BuildUnconfiguredProvider().RegisterAsync(
            new DomainRegistrationOrder("example.com", 1, false, false, null, null), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
        namecheap.Requests.Should().BeEmpty();
    }

    /// <summary>An ERROR answer is a clean refusal carrying Namecheap's message, not an exception.</summary>
    [Fact]
    public async Task RegisterAsync_WhenNamecheapAnswersError_FailsCleanlyAsync()
    {
        var provider = BuildProvider(ErrorResponse);

        var result = await provider.RegisterAsync(
            new DomainRegistrationOrder("example.com", 1, false, false, null, null), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Domain is not available");
    }

    /// <summary>An OK envelope without <c>Registered="true"</c> registered nothing.</summary>
    [Fact]
    public async Task RegisterAsync_WithoutRegisteredConfirmation_FailsAsync()
    {
        var provider = BuildProvider("""<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse><DomainCreateResult Domain="example.com" Registered="false" /></CommandResponse></ApiResponse>""");

        var result = await provider.RegisterAsync(
            new DomainRegistrationOrder("example.com", 1, false, false, null, null), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("did not confirm");
    }

    /// <summary>
    /// A confirmed registration without a <c>DomainID</c> is referenced by its name, which every
    /// later Namecheap call addresses it by, so activation never receives an empty reference.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithoutDomainId_FallsBackToTheDomainNameAsync()
    {
        var provider = BuildProvider("""<?xml version="1.0"?><ApiResponse Status="OK"><CommandResponse><DomainCreateResult Domain="example.com" Registered="true" /></CommandResponse></ApiResponse>""");

        var result = await provider.RegisterAsync(
            new DomainRegistrationOrder("example.com", 2, false, false, null, null), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.RegistrarRef.Should().Be("example.com");
    }

    /// <summary>A transfer on an unconfigured integration is refused.</summary>
    [Fact]
    public async Task TransferAsync_WhenNotConfigured_FailsAsync()
    {
        var result = await BuildUnconfiguredProvider().TransferAsync(
            new DomainTransferOrder("example.com", "EPP-1", false), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
    }

    /// <summary>A transfer Namecheap refuses with ERROR is a clean refusal.</summary>
    [Fact]
    public async Task TransferAsync_WhenNamecheapAnswersError_FailsCleanlyAsync()
    {
        var result = await BuildProvider(ErrorResponse).TransferAsync(
            new DomainTransferOrder("example.com", "EPP-1", false), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Domain is not available");
    }

    /// <summary>
    /// A renewal Namecheap refuses with ERROR is a clean refusal, so the paid-renewal engine
    /// releases the claim and retries instead of parking it as an unknown outcome.
    /// </summary>
    [Fact]
    public async Task RenewAsync_WhenNamecheapAnswersError_FailsCleanlyAsync()
    {
        var result = await BuildProvider(ErrorResponse).RenewAsync(
            new DomainRenewalOrder("example.com", "example.com", 1), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Domain is not available");
    }

    /// <summary>A change to a registrar-held domain on an unconfigured integration is refused.</summary>
    [Fact]
    public async Task SetNameserversAsync_WhenNotConfigured_FailsAsync()
    {
        var result = await BuildUnconfiguredProvider().SetNameserversAsync(
            "example.com", "example.com", ["ns1.example.test"], CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
    }

    /// <summary>
    /// A DNS record added on an unconfigured integration is refused, so the handler does not save
    /// a record Namecheap never got.
    /// </summary>
    [Fact]
    public async Task AddDnsRecordAsync_WhenNotConfigured_FailsAsync()
    {
        var record = DnsRecord.CreateDetached(DnsRecordType.A, "www", "192.0.2.1", 1800, null);

        var result = await BuildUnconfiguredProvider().AddDnsRecordAsync(
            "example.com", "example.com", record, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not configured");
        namecheap.Requests.Should().BeEmpty();
    }

    /// <summary>The client refuses to answer an unconfigured call with a fake OK envelope.</summary>
    [Fact]
    public async Task Client_WhenNotConfigured_ThrowsInsteadOfFakingSuccessAsync()
    {
        var unconfigured = Settings;
        unconfigured.ApiKey = string.Empty;
        var client = new NamecheapClient(new HttpClient(), Options.Create(unconfigured));

        var act = () => client.ExecuteAsync("namecheap.domains.getList", [], CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
