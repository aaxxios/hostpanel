using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Auth;
/// <summary>
/// Unit tests for the SSO-backed identity provider, against a stub SSO HTTP server.
///
/// <para>
/// No database and no container: what is worth checking here is how this product reads
/// the SSO's replies, and above all that it tells "no such account" apart from "the SSO
/// did not answer". Confusing those two would let a network fault read as a deleted
/// customer.
/// </para>
/// </summary>
public sealed class SsoIdentityProviderTests : IAsyncLifetime
{
    /// <summary>The loopback server standing in for the SSO; started per test class instance.</summary>
    private StubHttpServer sso = null!;

    /// <summary>Starts the stub SSO server before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        sso = await StubHttpServer.StartAsync();
    }

    /// <summary>Stops the stub SSO server after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        await sso.DisposeAsync();
    }

    /// <summary>Builds a provider over a client aimed at the stub server, which replays the given answers in order.</summary>
    /// <param name="replies">The scripted answers; none means every call is answered 404.</param>
    /// <returns>The provider under test.</returns>
    private SsoIdentityProvider Build(params StubReply[] replies)
    {
        foreach (var reply in replies)
        {
            sso.Enqueue(reply);
        }

        var http = new HttpClient { BaseAddress = sso.BaseAddress };
        return new SsoIdentityProvider(new SsoServiceClient(http));
    }

    /// <summary>Scripts a JSON answer.</summary>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="body">The JSON body.</param>
    /// <returns>The scripted reply.</returns>
    private static StubReply Json(HttpStatusCode status, string body) => new(status, body);

    /// <summary>Scripts an answer with the given status and an empty body.</summary>
    /// <param name="status">The HTTP status code.</param>
    /// <returns>The scripted reply.</returns>
    private static StubReply Status(HttpStatusCode status) => new(status, string.Empty);

    /// <summary>Verifies: Find By Subject Async - Returns The Account.</summary>
    [Fact]
    public async Task FindBySubjectAsync_ReturnsTheAccountAsync()
    {
        var provider = Build(Json(HttpStatusCode.OK,
            """{"email":"ada@example.com","name":"Ada Lovelace","firstName":"Ada","lastName":"Lovelace"}"""));

        var found = await provider.FindBySubjectAsync("sub-1", CancellationToken.None);

        found.Should().NotBeNull();
        found!.Subject.Should().Be("sub-1");
        found.Email.Should().Be("ada@example.com");
        found.FirstName.Should().Be("Ada");
        found.LastName.Should().Be("Lovelace");
    }

    /// <summary>Verifies: Find By Subject Async - When The Sso Has No Such Account - Returns Null.</summary>
    [Fact]
    public async Task FindBySubjectAsync_WhenTheSsoHasNoSuchAccount_ReturnsNullAsync()
    {
        var provider = Build(Status(HttpStatusCode.NotFound));

        /// <summary>Find By Subject Async, when The Service Key Is Rejected, throws.</summary>
        var found = await provider.FindBySubjectAsync("sub-unknown", CancellationToken.None);

        found.Should().BeNull();
    }

    /// <summary>Verifies: Find By Subject Async - When The Sso Fails - Throws Rather Than Returning Null.</summary>
    [Fact]
    public async Task FindBySubjectAsync_WhenTheSsoFails_ThrowsRatherThanReturningNullAsync()
    {
        // The case this class exists for. A 500 means the answer is unknown; returning
        // null would tell the caller the person does not exist, and the admin screens
        // would show a live customer as an unresolved row.
        /// <summary>Find By Email Async, resolves Through The Lookup Then The Account.</summary>
        var provider = Build(Status(HttpStatusCode.InternalServerError));

        var act = () => provider.FindBySubjectAsync("sub-1", CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    /// <summary>Verifies: Find By Subject Async - When The Service Key Is Rejected - Throws.</summary>
    [Fact]
    public async Task FindBySubjectAsync_WhenTheServiceKeyIsRejected_ThrowsAsync()
    {
        // A misconfigured key is a deployment fault. Silently reporting every account as
        // missing is how that fault would look like data loss instead.
        var provider = Build(Status(HttpStatusCode.Unauthorized));

        var act = () => provider.FindBySubjectAsync("sub-1", CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    /// <summary>Verifies: Find By Email Async - Resolves Through The Lookup Then The Account.</summary>
    [Fact]
    public async Task FindByEmailAsync_ResolvesThroughTheLookupThenTheAccountAsync()
    {
        // Two calls in order: the lookup answers the subject, then the account answers the profile.
        var provider = Build(
            Json(HttpStatusCode.OK, """{"userId":"sub-42"}"""),
            Json(HttpStatusCode.OK,
                """{"email":"grace@example.com","name":"Grace Hopper","firstName":"Grace","lastName":"Hopper"}"""));

        var found = await provider.FindByEmailAsync("grace@example.com", CancellationToken.None);

        found.Should().NotBeNull();
        found!.Subject.Should().Be("sub-42");
        found.Email.Should().Be("grace@example.com");
        sso.Requests.Should().HaveCount(2);
    }

    /// <summary>Verifies: Find By Email Async - For An Unknown Address - Returns Null Without ASecond Call.</summary>
    [Fact]
    public async Task FindByEmailAsync_ForAnUnknownAddress_ReturnsNullWithoutASecondCallAsync()
    {
        var provider = Build(Status(HttpStatusCode.NotFound));

        var found = await provider.FindByEmailAsync("nobody@example.com", CancellationToken.None);

        found.Should().BeNull();
        sso.Requests.Should().ContainSingle();
    }

    /// <summary>Verifies: Find By Email Async - Escapes The Address.</summary>
    /// <summary>Get Emails By Subjects Async, returns What The Sso Knows.</summary>
    [Fact]
    public async Task FindByEmailAsync_EscapesTheAddressAsync()
    {
        // Addresses contain characters a query string treats as structure. Unescaped,
        // "a+b@example.com" reaches the SSO as "a b@example.com" and resolves to nobody.
        var provider = Build(Status(HttpStatusCode.NotFound));

        await provider.FindByEmailAsync("a+b@example.com", CancellationToken.None);

        sso.Requests[0].PathAndQuery.Should().Contain("a%2Bb%40example.com");
    }

    /// <summary>The batch reply, used by both bulk lookups below.</summary>
    /// <summary>Get Accounts By Subjects Async, returns Whole Accounts In One Call.</summary>
    private const string TwoAccounts =
        """
        {"users":[
          {"id":"sub-1","email":"ada@example.com","firstName":"Ada","lastName":"Lovelace","twoFactorEnabled":true},
          {"id":"sub-2","email":"grace@example.com","firstName":"Grace","lastName":"Hopper","twoFactorEnabled":false}]}
        """;

    /// <summary>Verifies: Get Emails By Subjects Async - Returns What The Sso Knows.</summary>
    [Fact]
    public async Task GetEmailsBySubjectsAsync_ReturnsWhatTheSsoKnowsAsync()
    {
        var provider = Build(Json(HttpStatusCode.OK, TwoAccounts));

        var emails = await provider.GetEmailsBySubjectsAsync(
            ["sub-1", "sub-2", "sub-3"], CancellationToken.None);

        emails.Should().HaveCount(2);
        emails["sub-1"].Should().Be("ada@example.com");
        emails.ContainsKey("sub-3").Should().BeFalse();
    }

    /// <summary>Verifies: Get Accounts By Subjects Async - Returns Whole Accounts In One Call.</summary>
    [Fact]
    public async Task GetAccountsBySubjectsAsync_ReturnsWholeAccountsInOneCallAsync()
    {
        var provider = Build(Json(HttpStatusCode.OK, TwoAccounts));

        var accounts = await provider.GetAccountsBySubjectsAsync(
            ["sub-1", "sub-2", "sub-3"], CancellationToken.None);

        accounts.Should().HaveCount(2);
        accounts["sub-1"].Email.Should().Be("ada@example.com");
        accounts["sub-1"].FirstName.Should().Be("Ada");
        accounts["sub-1"].TwoFactorEnabled.Should().BeTrue();
        accounts["sub-2"].TwoFactorEnabled.Should().BeFalse();
        accounts.ContainsKey("sub-3").Should().BeFalse();

        // One request for the set, not one per subject — the whole reason this exists.
        sso.Requests.Should().ContainSingle();
    }

    /// <summary>Verifies: Get Accounts By Subjects Async - When The Sso Fails - Throws.</summary>
    /// <summary>List Async, returns The Page And The Unpaged Total.</summary>
    [Fact]
    public async Task GetAccountsBySubjectsAsync_WhenTheSsoFails_ThrowsAsync()
    {
        // Same rule as the single lookup: an empty page would read as "none of these
        // people exist", and every row on the screen would show as an orphan.
        var provider = Build(Status(HttpStatusCode.BadGateway));

        var act = () => provider.GetAccountsBySubjectsAsync(["sub-1"], CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    /// <summary>Verifies: Get Emails By Subjects Async - For No Subjects - Makes No Request.</summary>
    [Fact]
    public async Task GetEmailsBySubjectsAsync_ForNoSubjects_MakesNoRequestAsync()
    {
        var provider = Build();

        var emails = await provider.GetEmailsBySubjectsAsync([], CancellationToken.None);

        emails.Should().BeEmpty();
        sso.Requests.Should().BeEmpty();
    }

    /// <summary>Verifies: List Async - Returns The Page And The Unpaged Total.</summary>
    [Fact]
    public async Task ListAsync_ReturnsThePageAndTheUnpagedTotalAsync()
    {
        var provider = Build(Json(HttpStatusCode.OK,
            """
            {"total":57,"page":2,"pageSize":2,"users":[
              {"id":"sub-1","email":"ada@example.com","firstName":"Ada","lastName":"Lovelace"},
              {"id":"sub-2","email":"grace@example.com","firstName":"Grace","lastName":"Hopper"}]}
            """));

        var (items, total) = await provider.ListAsync("lace", page: 2, pageSize: 2, CancellationToken.None);

        total.Should().Be(57);
        items.Should().HaveCount(2);
        items[0].Subject.Should().Be("sub-1");

        var query = sso.Requests.Single().PathAndQuery;
        query.Should().Contain("page=2").And.Contain("pageSize=2").And.Contain("search=lace");
    }

    /// <summary>Verifies: List Async - Without ASearch - Sends No Search Parameter.</summary>
    [Fact]
    public async Task ListAsync_WithoutASearch_SendsNoSearchParameterAsync()
    {
        var provider = Build(Json(HttpStatusCode.OK,
            """{"total":0,"page":1,"pageSize":25,"users":[]}"""));

        await provider.ListAsync(null, page: 1, pageSize: 25, CancellationToken.None);

        sso.Requests.Single().PathAndQuery.Should().NotContain("search=");
    }

    /// <summary>Verifies: List Async - When The Sso Fails - Throws Rather Than Returning An Empty Page.</summary>
    [Fact]
    public async Task ListAsync_WhenTheSsoFails_ThrowsRatherThanReturningAnEmptyPageAsync()
    {
        // An empty page reads as "this deployment has no users", which is a far more
        // alarming and far less actionable thing to show than an error.
        var provider = Build(Status(HttpStatusCode.BadGateway));

        var act = () => provider.ListAsync(null, 1, 25, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
