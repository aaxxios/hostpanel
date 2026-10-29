namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A handler that sends every request to a <see cref="StubHttpServer"/> whatever host and port it
/// names, keeping the path, query and headers, and remembers the address each request was
/// originally aimed at. It lets an adapter that builds its own <c>https://host:port</c> address
/// (so a test cannot choose the port) be exercised over real HTTP, and lets a test assert on the
/// address the adapter built.
/// </summary>
public sealed class RedirectToStubHandler : DelegatingHandler
{
    /// <summary>The stub server's base address, with a trailing slash.</summary>
    private readonly Uri _target;

    /// <summary>The addresses requests were originally aimed at, in order.</summary>
    private readonly List<Uri> _requestedUris = [];

    /// <summary>Initializes a new instance of the <see cref="RedirectToStubHandler"/> class.</summary>
    /// <param name="target">The stub server's base address.</param>
    public RedirectToStubHandler(Uri target)
        : base(new HttpClientHandler())
    {
        _target = target;
    }

    /// <summary>Gets the addresses requests were originally aimed at, in order.</summary>
    public IReadOnlyList<Uri> RequestedUris => _requestedUris;

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requestedUris.Add(request.RequestUri!);
        request.RequestUri = new Uri(_target, request.RequestUri!.PathAndQuery.TrimStart('/'));
        return base.SendAsync(request, cancellationToken);
    }
}
