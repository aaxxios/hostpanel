using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Replaces <c>[ApiController]</c>'s stock model-binding failure body, a bare
/// <c>ValidationProblemDetails</c> whose per-field text is raw deserializer output, with the same
/// localized <see cref="ErrorResponse"/> every other refusal answers with.
/// </summary>
/// <remarks>
/// <c>[ApiController]</c>'s model-validation filter short-circuits before any handler or controller
/// action runs, so neither <c>ExceptionHandlingMiddleware</c> nor <c>BaseApiController</c> is
/// consulted for this status. It is its own file under <c>Shared/</c>, like the other error
/// surfaces (<c>ErrorResponse</c>, <c>BaseApiController</c>, <c>ExceptionHandlingMiddleware</c>,
/// <c>ProblemDetailsAuthorizationResponseExtensions</c>, <c>RateLimitingExtensions</c>), so it is
/// testable on its own, the way <see cref="RateLimitingExtensions"/>' rejection writer is, without
/// pulling in the rest of the API layer's registrations.
/// </remarks>
public static class ModelStateProblemDetailsExtensions
{
    /// <summary>
    /// Substrings that mark a model-state message as deserializer or CLR output rather than a
    /// person-facing sentence: a bad enum value or a malformed date answers with one of these
    /// verbatim unless this factory intervenes. Includes this product's own namespace prefix;
    /// <c>System.</c> is kept because a <c>System.Text.Json</c> message is
    /// just as much machinery as one naming an internal type.
    /// </summary>
    private static readonly string[] InternalMarkers =
    [
        "Innovayse.",
        "System.",
        "could not be converted",
        "JSON value",
        "LineNumber:",
        "BytePositionInLine:",
    ];

    /// <summary>
    /// The body this factory answers with when producing the localized one fails for any reason.
    /// The one literal sentence in this file, and deliberately so: it exists for the case where the
    /// localizer itself could not be resolved or a resource key is missing, so a resx lookup is not
    /// something this fallback can depend on.
    /// </summary>
    private static readonly ErrorResponse FallbackResponse = new(
        SharedErrorCodes.ValidationError,
        "Validation Failed",
        StatusCodes.Status400BadRequest,
        "One or more validation errors occurred.");

    /// <summary>
    /// Sets <see cref="ApiBehaviorOptions.InvalidModelStateResponseFactory"/> to build a
    /// <c>VALIDATION_ERROR</c> <see cref="ErrorResponse"/> instead of the framework's bare
    /// <c>ValidationProblemDetails</c>.
    /// </summary>
    /// <param name="services">The container.</param>
    /// <returns>The container, for chaining.</returns>
    public static IServiceCollection ConfigureModelStateProblemDetails(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.Configure<ApiBehaviorOptions>(
            options => options.InvalidModelStateResponseFactory = BuildProblemDetails);
    }

    /// <summary>
    /// Builds the 400 body for a model-binding failure. Never throws: this factory runs inside the
    /// MVC pipeline with nothing downstream to catch it, so a missing resource key or an
    /// unavailable localizer falls back to <see cref="FallbackResponse"/> rather than turning a
    /// handled 400 into an unhandled 500.
    /// </summary>
    /// <param name="context">The failed action's context.</param>
    /// <returns>The error result.</returns>
    private static IActionResult BuildProblemDetails(ActionContext context)
    {
        try
        {
            return BuildProblemDetailsCore(context);
        }
        catch (Exception)
        {
            // Deliberately broad: whatever failed above (DI resolution, a resource lookup), the
            // contract this factory must keep is "never throws", not "throws only known exceptions".
            return ErrorBody.ToActionResult(FallbackResponse);
        }
    }

    /// <summary>The real construction, kept separate so the try/catch above reads as a single guard.</summary>
    /// <param name="context">The failed action's context.</param>
    /// <returns>The error result.</returns>
    private static IActionResult BuildProblemDetailsCore(ActionContext context)
    {
        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<ValidationMessages>>();
        var errors = new Dictionary<string, List<string>>();

        foreach (var entry in context.ModelState)
        {
            if (entry.Value is null || entry.Value.Errors.Count == 0)
            {
                continue;
            }

            var key = NormalizeFieldKey(entry.Key);
            var messages = entry.Value.Errors.Select(error => SanitizeMessage(error, localizer)).ToList();

            if (errors.TryGetValue(key, out var existing))
            {
                existing.AddRange(messages);
            }
            else
            {
                errors[key] = messages;
            }
        }

        var body = new ErrorResponse(
            SharedErrorCodes.ValidationError,
            localizer["ValidationFailed"],
            StatusCodes.Status400BadRequest,
            localizer["ValidationErrorsOccurred"],
            errors);

        return ErrorBody.ToActionResult(body);
    }

    /// <summary>
    /// One field-level message, sanitized. A binder failure always carries a CLR
    /// <see cref="ModelError.Exception"/> and never a validator's sentence, so its text is replaced
    /// unconditionally; a plain <see cref="ModelStateDictionary"/> entry (no exception) keeps its
    /// own message unless it still contains one of <see cref="InternalMarkers"/> as a defence in depth.
    /// </summary>
    /// <param name="error">The model error.</param>
    /// <param name="localizer">The request's localizer.</param>
    /// <returns>The message to put on the wire.</returns>
    private static string SanitizeMessage(ModelError error, IStringLocalizer<ValidationMessages> localizer)
    {
        if (error.Exception is not null || ContainsInternalMarker(error.ErrorMessage))
        {
            return localizer["InvalidFieldValue"];
        }

        return error.ErrorMessage;
    }

    /// <summary>Whether a message carries one of <see cref="InternalMarkers"/>.</summary>
    /// <param name="message">The raw model-state message.</param>
    /// <returns>True when the message leaks internal machinery.</returns>
    private static bool ContainsInternalMarker(string message)
    {
        foreach (var marker in InternalMarkers)
        {
            if (message.Contains(marker, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Collapses a <c>System.Text.Json</c> path (<c>$.tier</c>, or <c>$</c> for the whole body) and
    /// an MVC property path (<c>Tier</c>, <c>Address.City</c>) to the same plain, camelCase field
    /// name, so a client sees one field-naming convention regardless of which binder rejected it.
    /// </summary>
    /// <param name="key">The raw <see cref="ModelStateDictionary"/> key.</param>
    /// <returns><c>"request"</c> for the whole body, otherwise the camelCase field path.</returns>
    private static string NormalizeFieldKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key == "$")
        {
            return "request";
        }

        var trimmed = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;
        var segments = trimmed.Split('.');
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = LowerFirstLetter(segments[i]);
        }

        return string.Join('.', segments);
    }

    /// <summary>
    /// Lower-cases the first character of a path segment, leaving an already-camelCase segment untouched.
    /// </summary>
    /// <param name="segment">One dot-separated segment of a field path.</param>
    /// <returns>The segment with its first letter lower-cased.</returns>
    private static string LowerFirstLetter(string segment)
    {
        if (segment.Length == 0 || !char.IsUpper(segment[0]))
        {
            return segment;
        }

        // Not string.Concat(char, ReadOnlySpan<char>): no such overload exists, so the compiler
        // binds Concat(object?, object?) and then refuses the span. A range expression says the
        // same thing without the trap.
        return char.ToLowerInvariant(segment[0]) + segment[1..];
    }
}
