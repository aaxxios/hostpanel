using System.Net.Mime;
using System.Text.Json;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// The one serializer every error surface writes through: <c>BaseApiController</c>, the exception
/// middleware, the model-state factory, the two auth writers and the rate limiter. One place means
/// the wire shape and the media type cannot drift between surfaces.
/// </summary>
public static class ErrorBody
{
    /// <summary>The media type of every error body (RFC 9457).</summary>
    public const string ContentType = MediaTypeNames.Application.ProblemJson;

    /// <summary>
    /// camelCase for the members and for the <c>errors</c> keys, the wire convention (rules/naming.md).
    /// Shared because serializer options cache metadata and are immutable once used.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Serializes <paramref name="body"/> exactly as the <see cref="ErrorResponse"/> declares it: no
    /// members beyond the standard ones.
    /// </summary>
    /// <param name="body">The error body.</param>
    /// <returns>The JSON text.</returns>
    public static string Serialize(ErrorResponse body)
    {
        ArgumentNullException.ThrowIfNull(body);

        return JsonSerializer.Serialize(body, SerializerOptions);
    }

    /// <summary>
    /// Writes <paramref name="body"/> as the response, unless the response has already started, in
    /// which case its status and headers can no longer change and nothing is written.
    /// </summary>
    /// <param name="context">The request.</param>
    /// <param name="body">The error body; its <c>Status</c> becomes the response status.</param>
    /// <param name="ct">Cancellation token; the request's own abort token where the caller has no other.</param>
    /// <returns><c>true</c> when the body was written.</returns>
    public static async Task<bool> WriteAsync(HttpContext context, ErrorResponse body, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(body);

        if (context.Response.HasStarted)
        {
            return false;
        }

        context.Response.StatusCode = body.Status;
        context.Response.ContentType = ContentType;
        await context.Response.WriteAsync(Serialize(body), ct);
        return true;
    }

    /// <summary>
    /// The same body as an MVC result, for the surfaces that answer with an <see cref="IActionResult"/>.
    /// </summary>

    /// <param name="body">The error body; its <c>Status</c> becomes the response status.</param>
    /// <returns>The result.</returns>
    public static IActionResult ToActionResult(ErrorResponse body)
    {
        ArgumentNullException.ThrowIfNull(body);

        return new ContentResult
        {
            StatusCode = body.Status,
            ContentType = ContentType,
            Content = Serialize(body),
        };
    }
}
