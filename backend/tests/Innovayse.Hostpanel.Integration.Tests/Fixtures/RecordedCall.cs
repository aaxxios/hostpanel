namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>One call a <see cref="StubHttpServer"/> received, kept so a test can assert on what the client sent.</summary>
/// <param name="Method">The HTTP method.</param>
/// <param name="PathAndQuery">The request path and query string, as sent.</param>
/// <param name="Body">The request body, empty when there was none.</param>
/// <param name="Authorization">The <c>Authorization</c> header as sent, or <see langword="null"/> when there was none.</param>
/// <param name="IdempotencyKey">The <c>Idempotency-Key</c> header as sent, or <see langword="null"/> when there was none.</param>
public sealed record RecordedCall(
    string Method,
    string PathAndQuery,
    string Body,
    string? Authorization,
    string? IdempotencyKey = null);
