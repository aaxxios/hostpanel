using System.Net.Http.Json;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Infrastructure.Integrations.Migration.Settings;
using Innovayse.Hostpanel.Infrastructure.Resilience;
using Innovayse.Hostpanel.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.Migration;

/// <summary>
/// Talks to the legacy system's migration plugin over HTTP: every operation is a JSON POST to the
/// job's configured URL carrying the job key and the name of the operation, and the plugin answers
/// with a JSON body. This is the only place in the product that knows the migration source speaks
/// HTTP at all.
/// </summary>
/// <remarks>
/// The URL is admin input this server calls, so it is checked before every request: HTTPS only
/// unless <see cref="MigrationSourceOptions.AllowInsecureHttp"/> is set, and the named client's
/// connection callback refuses internal addresses (<see cref="OutboundConnector"/>, with the migration
/// source's own private network setting). A refused scheme or address throws
/// <see cref="MigrationSourceAddressRefusedException"/>, so the job records it as a source address that
/// is not allowed, not as an unreachable source.
/// </remarks>
public sealed class MigrationSourceClient : IMigrationSource
{
    /// <summary>
    /// Name of the configured <see cref="HttpClient"/>. It carries a deliberately long timeout,
    /// because a single page of a large export can take minutes to assemble on the source side.
    /// </summary>
    private const string HttpClientName = HttpClientNames.Migration;

    /// <summary>Operation name the source expects for a bare connectivity check.</summary>
    private const string PingAction = "ping";

    /// <summary>Operation name the source expects when asked for its per-kind record counts.</summary>
    private const string TotalsAction = "totals";

    /// <summary>
    /// How responses are read. Case-insensitive because the source names its fields in its own
    /// convention rather than ours, and a mismatch there would silently yield zeroes and nulls.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Factory for the named client that carries the pull timeout, the size cap and the address check.</summary>
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>Which source addresses are accepted and how large an answer may be.</summary>
    private readonly MigrationSourceOptions _options;

    /// <summary>Logger used to record a refused address or a refused request.</summary>
    private readonly ILogger<MigrationSourceClient> _logger;

    /// <summary>Initializes a new instance of the <see cref="MigrationSourceClient"/> class.</summary>
    /// <param name="httpClientFactory">Factory for the named client that carries the pull timeout, the size cap and the address check.</param>
    /// <param name="options">Which source addresses are accepted and how large an answer may be.</param>
    /// <param name="logger">Logger used to record a refused address or a refused request.</param>
    public MigrationSourceClient(
        IHttpClientFactory httpClientFactory, IOptions<MigrationSourceOptions> options, ILogger<MigrationSourceClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PingAsync(MigrationSourceEndpoint source, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(source);

        using var response = await PostAsync(source, new MigrationSourceActionWire(source.AccessKey, PingAction), PingAction, ct);
    }

    /// <inheritdoc />
    public async Task<MigrationSourceTotals> GetTotalsAsync(MigrationSourceEndpoint source, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(source);

        var wire = await PostActionAsync<MigrationSourceActionWire, MigrationSourceTotalsWire>(
            source, new MigrationSourceActionWire(source.AccessKey, TotalsAction), TotalsAction, ct);
        return wire.ToTotals();
    }

    /// <inheritdoc />
    public Task<MigrationRecordPage<TRecord>> GetRecordPageAsync<TRecord>(
        MigrationSourceEndpoint source, MigrationPageSelection selection, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selection);

        var wire = new MigrationSourcePageWire(
            source.AccessKey, selection.DataSet, selection.Page, selection.PerPage, selection.AfterId);
        return PostActionAsync<MigrationSourcePageWire, MigrationRecordPage<TRecord>>(source, wire, selection.DataSet, ct);
    }

    /// <summary>Posts one operation to the source and reads its answer, within the size cap.</summary>
    /// <typeparam name="TWire">The body sent to the source.</typeparam>
    /// <typeparam name="T">Shape the answer is read back as.</typeparam>
    /// <param name="source">Where the source listens.</param>
    /// <param name="wire">The body to send.</param>
    /// <param name="action">The operation's name, for the log.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The deserialized answer.</returns>
    /// <exception cref="MigrationSourceAddressRefusedException">The URL or the address it resolves to is not allowed.</exception>
    /// <exception cref="HttpRequestException">The source answered a non-success status, or the answer exceeded the size cap.</exception>
    /// <exception cref="InvalidOperationException">The source answered a body that read as null.</exception>
    private async Task<T> PostActionAsync<TWire, T>(MigrationSourceEndpoint source, TWire wire, string action, CancellationToken ct)
    {
        using var response = await PostAsync(source, wire, action, ct);

        // Read into a bounded buffer: the headers were read alone, so nothing has been buffered yet
        // and a body over the cap fails here instead of filling memory.
        await response.Content.LoadIntoBufferAsync(_options.MaxResponseBytes, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOpts, ct)
            ?? throw new InvalidOperationException($"Empty response for action '{action}'.");
    }

    /// <summary>Checks the address, posts the body and returns a successful response with only its headers read.</summary>
    /// <typeparam name="TWire">The body sent to the source.</typeparam>
    /// <param name="source">Where the source listens.</param>
    /// <param name="wire">The body to send.</param>
    /// <param name="action">The operation's name, for the log.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response; the caller disposes it.</returns>
    /// <exception cref="MigrationSourceAddressRefusedException">The URL or the address it resolves to is not allowed.</exception>
    /// <exception cref="HttpRequestException">The source could not be reached or answered a non-success status.</exception>
    private async Task<HttpResponseMessage> PostAsync<TWire>(
        MigrationSourceEndpoint source, TWire wire, string action, CancellationToken ct)
    {
        var uri = RequireAllowedUri(source.Url);
        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = JsonContent.Create(wire) };
        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (HttpRequestException ex) when (ex.InnerException is OutboundAddressRefusedException)
        {
            _logger.LogWarning("Migration source address refused: it resolves only to addresses that are not allowed.");
            throw new MigrationSourceAddressRefusedException(ex);
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        // The error body is never read: it can carry client records, and an unread body costs
        // nothing. Only the status and the declared length reach the log.
        _logger.LogError(
            "Migration source returned {Status} for action '{Action}', declared body length {Length}",
            (int)response.StatusCode, action, response.Content.Headers.ContentLength);
        var status = response.StatusCode;
        response.Dispose();
        throw new HttpRequestException($"The migration source answered {(int)status}.", inner: null, status);
    }

    /// <summary>Parses the source URL and refuses a scheme the options do not allow.</summary>
    /// <param name="url">The job's source URL.</param>
    /// <returns>The parsed address.</returns>
    /// <exception cref="MigrationSourceAddressRefusedException">The URL is not absolute, or not HTTPS while plain HTTP is not allowed.</exception>
    private Uri RequireAllowedUri(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && _options.AllowInsecureHttp)))
        {
            return uri;
        }

        _logger.LogWarning("Migration source URL refused: only HTTPS is allowed.");
        throw new MigrationSourceAddressRefusedException();
    }
}
