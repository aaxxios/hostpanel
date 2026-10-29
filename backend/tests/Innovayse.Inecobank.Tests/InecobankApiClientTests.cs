using System.Net;
using System.Text.Json;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Providers.Inecobank;
using Microsoft.Extensions.Logging.Abstractions;

namespace Innovayse.Inecobank.Tests;

/// <summary>Tests for <see cref="InecobankApiClient"/> request encoding and response parsing.</summary>
public class InecobankApiClientTests : IAsyncLifetime
{
    /// <summary>The loopback server standing in for the gateway; the client's requests are redirected to it.</summary>
    private StubHttpServer _http = null!;

    /// <summary>Starts the stub gateway before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        _http = await StubHttpServer.StartAsync();
    }

    /// <summary>Stops the stub gateway after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        await _http.DisposeAsync();
    }

    /// <summary>Builds the HTTP client every request of which reaches the stub gateway.</summary>
    /// <returns>The client.</returns>
    private HttpClient CreateHttpClient() => new(new RedirectToStubHandler(_http.BaseAddress));

    /// <summary>Client options pointing at a non-existent test endpoint.</summary>
    private static readonly InecobankClientOptions Options =
        new("https://testpg.example.am", "merchant-api", "secret-pw");

    /// <summary>Builds a client wired to the stub gateway.</summary>
    /// <returns>The client under test; the stub gateway's responses and requests are controlled through the test's server.</returns>
    private InecobankApiClient CreateClient()
    {
        return new InecobankApiClient(CreateHttpClient(), Options, NullLogger.Instance);
    }

    /// <summary>Register order async, sends credentials amount and explicit currency.</summary>
    [Fact]
    public async Task RegisterOrderAsync_SendsCredentialsAmountAndExplicitCurrency()
    {
        var client = CreateClient();
        _http.Enqueue("""{"orderId":"32faa424-858a","formUrl":"https://testpg.example.am/pay?mdOrder=32faa424-858a"}""");

        var result = await client.RegisterOrderAsync(
            new InecobankRegisterRequest("INV42-1755850000", 250000, "051",
                "https://portal.example.com/payment/result?invoice=42", "Invoice #42", "hy"),
            CancellationToken.None);

        result.OrderId.Should().Be("32faa424-858a");
        result.FormUrl.Should().Be("https://testpg.example.am/pay?mdOrder=32faa424-858a");
        _http.Requests.Should().ContainSingle();
        _http.Requests[0].PathAndQuery.Should().Be("/payment/rest/register.do");
        var body = _http.Requests[0].Body;
        body.Should().Contain("userName=merchant-api");
        body.Should().Contain("password=secret-pw");
        body.Should().Contain("orderNumber=INV42-1755850000");
        body.Should().Contain("amount=250000");
        body.Should().Contain("currency=051"); // gateway defaults to 643 (RUB) when omitted, so it must always be explicit
        body.Should().Contain("language=hy");
    }

    /// <summary>Register order async, sanitizes forbidden description characters.</summary>
    [Fact]
    public async Task RegisterOrderAsync_SanitizesForbiddenDescriptionCharacters()
    {
        var client = CreateClient();
        _http.Enqueue("""{"orderId":"o1","formUrl":"https://x/pay"}""");

        await client.RegisterOrderAsync(
            new InecobankRegisterRequest("INV1-1", 100, "051", "https://x/r",
                "50% off + extras\r\nline2" + new string('x', 120), null),
            CancellationToken.None);

        var body = System.Net.WebUtility.UrlDecode(_http.Requests[0].Body);
        var description = body.Split('&').First(p => p.StartsWith("description="))["description=".Length..];
        description.Should().NotContainAny("%", "+", "\r", "\n");
        description.Length.Should().BeLessThanOrEqualTo(99);
    }

    /// <summary>Register order async, non zero error code, throws with code.</summary>
    [Fact]
    public async Task RegisterOrderAsync_NonZeroErrorCode_ThrowsWithCode()
    {
        var client = CreateClient();
        _http.Enqueue("""{"errorCode":"1","errorMessage":"Order already processed"}""");

        var act = () => client.RegisterOrderAsync(
            new InecobankRegisterRequest("INV1-1", 100, "051", "https://x/r", null, null),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InecobankApiException>();
        ex.Which.ErrorCode.Should().Be(1);
    }

    /// <summary>Get order status async, parses string and number error codes.</summary>
    /// <param name="json">The gateway response body.</param>
    /// <param name="expectedError">Error code that should be parsed.</param>
    /// <param name="expectedStatus">Order status that should be parsed, or null.</param>
    /// <param name="expectedRef">Authorization reference that should be parsed, or null.</param>
    [Theory]
    [InlineData("""{"errorCode":0,"orderStatus":2,"authRefNum":"ref-777"}""", 0, 2, "ref-777")]
    [InlineData("""{"errorCode":"0","orderStatus":0}""", 0, 0, null)]
    [InlineData("""{"errorCode":6,"errorMessage":"Незарегистрированный OrderId"}""", 6, null, null)]
    public async Task GetOrderStatusAsync_ParsesStringAndNumberErrorCodes(
        string json, int expectedError, int? expectedStatus, string? expectedRef)
    {
        var client = CreateClient();
        _http.Enqueue(json);

        var status = await client.GetOrderStatusAsync("some-order", "hy", CancellationToken.None);

        status.ErrorCode.Should().Be(expectedError);
        status.OrderStatus.Should().Be(expectedStatus);
        status.AuthRefNum.Should().Be(expectedRef);
        _http.Requests[0].PathAndQuery.Should().Be("/payment/rest/getOrderStatusExtended.do");
    }

    /// <summary>Refund async, sends order id and minor amount, throws on error.</summary>
    [Fact]
    public async Task RefundAsync_SendsOrderIdAndMinorAmount_ThrowsOnError()
    {
        var client = CreateClient();
        _http.Enqueue("""{"errorCode":0}""");
        await client.RefundAsync("order-9", 5000, CancellationToken.None);
        _http.Requests[0].PathAndQuery.Should().Be("/payment/rest/refund.do");
        _http.Requests[0].Body.Should().Contain("orderId=order-9").And.Contain("amount=5000");

        _http.Enqueue("""{"errorCode":7,"errorMessage":"System error"}""");
        var act = () => client.RefundAsync("order-9", 5000, CancellationToken.None);
        (await act.Should().ThrowAsync<InecobankApiException>()).Which.ErrorCode.Should().Be(7);
    }

    // The live gateway answers ordinary business outcomes with a 500 and a JSON body that names
    // the problem, so the status alone is not the verdict and the body is read first. What this
    // test protects: a response this client cannot make sense of
    // must still surface as its own exception type, naming the endpoint, with the underlying
    // failure attached rather than thrown raw at the caller. The status code is asserted too,
    // because when the body explains nothing the status is the only diagnostic left.
    /// <summary>Non-success status with an unreadable body: the failure is wrapped and reports the status.</summary>
    [Fact]
    public async Task RegisterOrderAsync_NonSuccessStatusWithAnUnreadableBody_WrapsTheFailureAndReportsTheStatus()
    {
        var client = CreateClient();
        _http.Enqueue(HttpStatusCode.BadGateway, "<html>upstream error</html>");

        var act = () => client.RegisterOrderAsync(
            new InecobankRegisterRequest("INV1-1", 100, "051", "https://x/r", null, null),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InecobankApiException>();
        ex.Which.InnerException.Should().BeAssignableTo<JsonException>();
        ex.Which.Message.Should().Contain(InecobankEndpoints.Register).And.Contain("502");
    }

    /// <summary>Non-JSON response body: the parse failure is wrapped rather than thrown raw.</summary>
    [Fact]
    public async Task RegisterOrderAsync_NonJsonResponseBody_WrapsJsonExceptionRatherThanThrowingItRaw()
    {
        var client = CreateClient();
        _http.Enqueue("this is not json");

        var act = () => client.RegisterOrderAsync(
            new InecobankRegisterRequest("INV1-1", 100, "051", "https://x/r", null, null),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InecobankApiException>();
        // JsonDocument.Parse throws JsonReaderException (a JsonException subclass) for
        // malformed syntax; assert the documented base type, not one specific derived type.
        ex.Which.InnerException.Should().BeAssignableTo<System.Text.Json.JsonException>();
        ex.Which.Message.Should().Contain(InecobankEndpoints.Register);
    }

    /// <summary>Operations, never log the configured merchant credentials.</summary>
    [Fact]
    public async Task Operations_NeverLogTheConfiguredMerchantCredentials()
    {
        var options = new InecobankClientOptions(
            "https://testpg.example.am", "very-secret-merchant-username", "very-secret-merchant-password");
        var logger = new CapturingLogger();
        var client = new InecobankApiClient(CreateHttpClient(), options, logger);

        _http.Enqueue("""{"orderId":"o1","formUrl":"https://x/pay"}""");
        await client.RegisterOrderAsync(
            new InecobankRegisterRequest("INV1-1", 100, "051", "https://x/r", "Invoice #1", "hy"),
            CancellationToken.None);

        _http.Enqueue("""{"errorCode":0,"orderStatus":2,"authRefNum":"ref-1"}""");
        await client.GetOrderStatusAsync("gw-order-1", "hy", CancellationToken.None);

        _http.Enqueue("""{"errorCode":0}""");
        await client.RefundAsync("gw-order-1", 500, CancellationToken.None);

        // Also exercise a failure path (wrapped in InecobankApiException per the tests above):
        // credentials must not leak into an exception message or a log call about it either.
        _http.Enqueue(HttpStatusCode.InternalServerError, "boom");
        await Assert.ThrowsAsync<InecobankApiException>(() => client.RefundAsync(
            "gw-order-1", 500, CancellationToken.None));

        var everythingLogged = logger.Messages.Concat(logger.Scopes).ToList();
        everythingLogged.Should().NotBeEmpty("the happy-path calls above do log informational messages");
        everythingLogged.Should().NotContain(m => m.Contains(options.UserName, StringComparison.Ordinal));
        everythingLogged.Should().NotContain(m => m.Contains(options.Password, StringComparison.Ordinal));
    }
}
