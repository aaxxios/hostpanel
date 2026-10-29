using System.Text.Json;
using System.Xml;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;

namespace Innovayse.Hostpanel.Infrastructure.Integrations;

/// <summary>
/// The one rule for which exceptions mean "the registrar did not do it" rather than "this code is
/// broken". An adapter turns the first kind into a failed result whose code the caller can act on,
/// logging the registrar's own wording and never returning it, and lets the second kind escape.
/// </summary>
internal static class RegistrarFailure
{
    /// <summary>
    /// Whether an exception is a failure of the registrar call itself: a refusal it sent, a
    /// transport failure or timeout, or an answer that is not the format it promised.
    /// </summary>
    /// <param name="exception">The exception an adapter call threw.</param>
    /// <param name="ct">The caller's cancellation token; a cancellation it asked for is not a failure.</param>
    /// <returns><see langword="true"/> when the exception describes the registrar call failing.</returns>
    public static bool IsUpstream(Exception exception, CancellationToken ct)
    {
        return exception is RegistrarException
            or HttpRequestException
            or XmlException
            or JsonException
            or TimeoutException
            || (exception is OperationCanceledException && !ct.IsCancellationRequested);
    }
}
