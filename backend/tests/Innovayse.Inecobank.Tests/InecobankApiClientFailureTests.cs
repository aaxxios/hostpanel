using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Providers.Inecobank;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Innovayse.Inecobank.Tests;

/// <summary>
/// <see cref="InecobankApiClient"/> must only ever throw <see cref="InecobankApiException"/>: the
/// failures of the resilience pipeline in front of it are translated, and a cancellation the
/// caller asked for stays a cancellation.
/// </summary>
public class InecobankApiClientFailureTests : IAsyncLifetime
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

    /// <summary>A timeout rejected by the resilience pipeline surfaces as a transport failure of the client's own type.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task RefundAsync_WrapsAPipelineTimeoutAsync()
    {
        var timeout = new TimeoutRejectedException("timed out");
        var client = CreateClient(timeout);

        var thrown = await Assert.ThrowsAsync<InecobankApiException>(() =>
            client.RefundAsync("gw-order-1", 500, CancellationToken.None));

        thrown.ErrorCode.Should().BeNegative();
        thrown.InnerException.Should().BeSameAs(timeout);
    }

    /// <summary>An open circuit surfaces as a transport failure of the client's own type.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task GetOrderStatusAsync_WrapsAnOpenCircuitAsync()
    {
        var open = new BrokenCircuitException("circuit open");
        var client = CreateClient(open);

        var thrown = await Assert.ThrowsAsync<InecobankApiException>(() =>
            client.GetOrderStatusAsync("gw-order-1", null, CancellationToken.None));

        thrown.ErrorCode.Should().BeNegative();
        thrown.InnerException.Should().BeSameAs(open);
    }

    /// <summary>The HTTP client's own timeout, which cancels the call without the caller asking, is a transport failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task RegisterOrderAsync_WrapsAnHttpClientTimeoutAsync()
    {
        var client = CreateClient(new TaskCanceledException("The request was canceled due to the configured timeout."));

        var thrown = await Assert.ThrowsAsync<InecobankApiException>(() => client.RegisterOrderAsync(
            new InecobankRegisterRequest("INV1-1", 100, "051", "https://x/r", "Invoice #1", "hy"),
            CancellationToken.None));

        thrown.ErrorCode.Should().BeNegative();
    }

    /// <summary>A cancellation the caller requested is not turned into a gateway failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task RefundAsync_LetsCallerCancellationPropagateAsync()
    {
        var client = CreateClient(new TaskCanceledException());
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.RefundAsync("gw-order-1", 500, cancelled.Token));
    }

    /// <summary>A status number too large for an integer reads as absent instead of throwing a non-gateway exception.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task GetOrderStatusAsync_ReadsAnOutOfRangeNumberAsAbsentAsync()
    {
        _http.Enqueue("""{"errorCode":0,"orderStatus":99999999999}""");
        var client = new InecobankApiClient(CreateHttpClient(), Options, NullLogger.Instance);

        var status = await client.GetOrderStatusAsync("gw-order-1", null, CancellationToken.None);

        status.OrderStatus.Should().BeNull();
        status.ErrorCode.Should().Be(0);
    }

    /// <summary>Builds a client whose every request fails with the given exception.</summary>
    /// <param name="failure">The exception the transport throws.</param>
    /// <returns>The client under test.</returns>
    private static InecobankApiClient CreateClient(Exception failure) =>
        new(new HttpClient(new ThrowingHttpMessageHandler(failure)), Options, NullLogger.Instance);
}
