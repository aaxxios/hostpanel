using System.Xml;
using System.Xml.Linq;
using Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap.Settings;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;

/// <summary>
/// Typed <see cref="HttpClient"/> wrapper for the Namecheap XML API v2.
/// Handles credential injection into the request body, request dispatch, and error mapping.
/// </summary>
public sealed class NamecheapClient
{
    /// <summary>The underlying HTTP client used to call the Namecheap API.</summary>
    private readonly HttpClient _http;

    /// <summary>Resolved Namecheap configuration settings.</summary>
    private readonly NamecheapOptions _settings;

    /// <summary>
    /// Initializes a new instance of <see cref="NamecheapClient"/>.
    /// </summary>
    /// <param name="http">The <see cref="HttpClient"/> configured by <c>IHttpClientFactory</c>.</param>
    /// <param name="options">Bound <see cref="NamecheapOptions"/> options.</param>
    public NamecheapClient(HttpClient http, IOptions<NamecheapOptions> options)
    {
        _http = http;
        _settings = options.Value;
    }

    /// <summary>
    /// Gets whether the Namecheap API is configured with valid credentials. Deferred to
    /// <see cref="NamecheapOptions.IsConfigured"/> so the rule for "configured" lives with the
    /// settings rather than being restated by each caller.
    /// </summary>
    public bool IsConfigured => _settings.IsConfigured;

    /// <summary>
    /// Executes a Namecheap XML API command and returns the parsed response document.
    /// </summary>
    /// <remarks>
    /// The request is a POST: the credentials and the command's parameters travel in the form body,
    /// so neither the API key nor the account name can end up in a URL, a proxy log or a client
    /// log. Only the command name is also put in the query string, where it is not a secret and
    /// lets the retry policy tell a lookup from a purchase. The command passed in is not changed.
    /// </remarks>
    /// <param name="command">The command and its parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Parsed XDocument of the successful API response.</returns>
    /// <exception cref="RegistrarException">Thrown when the API response contains ERROR status.</exception>
    /// <exception cref="HttpRequestException">
    /// Thrown on a transport failure, a non-success HTTP status, or an answer that is not XML.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the client is not configured. Callers check <see cref="IsConfigured"/> first;
    /// a path that forgets fails loudly instead of being answered with a fake success.
    /// </exception>
    public async Task<XDocument> ExecuteAsync(NamecheapCommand command, CancellationToken ct)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                $"Namecheap is not configured; '{command.Name}' was not sent.");
        }

        // A copy, so the caller's parameters are never written to; the credentials are set last so
        // a parameter of the same name cannot replace them.
        var form = new Dictionary<string, string>(command.Parameters)
        {
            ["ApiUser"] = _settings.ApiUser,
            ["ApiKey"] = _settings.ApiKey,
            ["UserName"] = _settings.ApiUser,
            ["ClientIp"] = _settings.ClientIp,
            ["Command"] = command.Name,
        };

        var url = $"{_settings.ApiUrl.TrimEnd('/')}?Command={Uri.EscapeDataString(command.Name)}";

        using var content = new FormUrlEncodedContent(form);
        using var response = await _http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(ct);

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            // Not what Namecheap promises (a gateway error page, say): the outcome of the command
            // is unknown, which is what a transport failure means to every caller.
            throw new HttpRequestException("Namecheap answered with something that is not XML.", ex);
        }

        var apiResponse = doc.Root;
        var status = apiResponse?.Attribute("Status")?.Value;

        if (string.Equals(status, "ERROR", StringComparison.OrdinalIgnoreCase))
        {
            var errorEl = apiResponse
                ?.Element(XName.Get("Errors", apiResponse.Name.NamespaceName))
                ?.Element(XName.Get("Error", apiResponse.Name.NamespaceName));

            var errorCode = errorEl?.Attribute("Number")?.Value ?? "0";
            var errorMessage = errorEl?.Value ?? "Unknown registrar error";

            throw RegistrarException.FromCode(errorCode, errorMessage);
        }

        return doc;
    }
}
