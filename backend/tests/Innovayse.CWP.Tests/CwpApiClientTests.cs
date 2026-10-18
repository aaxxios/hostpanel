using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Providers.CWP;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Innovayse.CWP.Tests;
/// <summary>Unit tests for <see cref="CwpApiClient"/> against a stub CWP HTTP server.</summary>
public sealed class CwpApiClientTests : IAsyncLifetime
{
    /// <summary>The loopback server standing in for the CWP host; started per test class instance.</summary>
    private StubHttpServer cwp = null!;

    /// <summary>Gets the stub server address without the trailing slash, as the client expects a base URL.</summary>
    private string BaseUrl => cwp.BaseAddress.ToString().TrimEnd('/');

    /// <summary>Starts the stub CWP server before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        cwp = await StubHttpServer.StartAsync();
    }

    /// <summary>Stops the stub CWP server after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        await cwp.DisposeAsync();
    }

    /// <summary>
    /// Creates a <see cref="CwpApiClient"/> aimed at the stub server, which answers the given JSON body over real HTTP.
    /// </summary>
    /// <param name="responseBody">JSON string to return as the HTTP response body.</param>
    /// <param name="statusCode">HTTP status code to return. Default 200 OK.</param>
    /// <returns>The configured <see cref="CwpApiClient"/>.</returns>
    private CwpApiClient BuildClient(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        cwp.Enqueue(statusCode, responseBody);
        return new CwpApiClient(new HttpClient(), NullLogger<CwpApiClient>.Instance);
    }

    /// <summary>CreateAccountAsync should return IsSuccess=true when CWP responds with status "OK".</summary>
    [Fact]
    public async Task CreateAccount_ReturnsTrue_WhenCwpRespondsOkAsync()
    {
        var client = BuildClient("""{ "status": "OK", "msj": "Account created." }""");

        var result = await client.CreateAccountAsync(
            BaseUrl, "test-key", "example.com", "testuser", "pass123", "default", "test@example.com",
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Account created.");
    }

    /// <summary>CreateAccountAsync should return IsSuccess=false when CWP responds with status "Error".</summary>
    [Fact]
    public async Task CreateAccount_ReturnsFalse_WhenCwpRespondsErrorAsync()
    {
        var client = BuildClient("""{ "status": "Error", "msj": "User already exists." }""");

        var result = await client.CreateAccountAsync(
            BaseUrl, "test-key", "example.com", "testuser", "pass123", "default", "test@example.com",
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User already exists.");
    }

    /// <summary>SuspendAccountAsync should return IsSuccess=true when CWP responds OK.</summary>
    [Fact]
    public async Task SuspendAccount_ReturnsTrue_WhenCwpRespondsOkAsync()
    {
        var client = BuildClient("""{ "status": "OK", "msj": "Account suspended." }""");

        var result = await client.SuspendAccountAsync(BaseUrl, "test-key", "testuser", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>UnsuspendAccountAsync should return IsSuccess=true when CWP responds OK.</summary>
    [Fact]
    public async Task UnsuspendAccount_ReturnsTrue_WhenCwpRespondsOkAsync()
    {
        var client = BuildClient("""{ "status": "OK", "msj": "Account unsuspended." }""");

        var result = await client.UnsuspendAccountAsync(BaseUrl, "test-key", "testuser", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>TerminateAccountAsync should return IsSuccess=true when CWP responds OK.</summary>
    [Fact]
    public async Task TerminateAccount_ReturnsTrue_WhenCwpRespondsOkAsync()
    {
        var client = BuildClient("""{ "status": "OK", "msj": "Account deleted." }""");

        var result = await client.TerminateAccountAsync(BaseUrl, "test-key", "testuser", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>SendAsync should throw HttpRequestException when the server returns 500.</summary>
    [Fact]
    public async Task CreateAccount_Throws_WhenServerReturns500Async()
    {
        var client = BuildClient("Internal Server Error", HttpStatusCode.InternalServerError);

        var act = () => client.CreateAccountAsync(
            BaseUrl, "test-key", "example.com", "testuser", "pass123", "default", "test@example.com",
            CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
