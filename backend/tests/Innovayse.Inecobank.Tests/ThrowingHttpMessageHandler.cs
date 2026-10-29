namespace Innovayse.Inecobank.Tests;

/// <summary>
/// An HTTP handler that fails every request with a scripted exception, standing in for the
/// resilience pipeline in front of the real client giving up.
/// </summary>
public sealed class ThrowingHttpMessageHandler : HttpMessageHandler
{
    /// <summary>The exception every request fails with.</summary>
    private readonly Exception _failure;

    /// <summary>Initializes the handler.</summary>
    /// <param name="failure">The exception every request fails with.</param>
    public ThrowingHttpMessageHandler(Exception failure)
    {
        _failure = failure;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromException<HttpResponseMessage>(_failure);
}
