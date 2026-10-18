using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using Innovayse.Hostpanel.Shared.Filters;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Shared.Controllers;

/// <summary>
/// The base of every controller: the members that turn a <see cref="Result"/> into HTTP. A
/// controller binds, dispatches once and hands the outcome here; it never names a status code
/// and never builds a body. Authorization is required by default so that a controller added
/// without an attribute is closed rather than open.
/// </summary>
[Authorize]
[ApiController]
[ServiceFilter(typeof(ActionTimingFilter))]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>Answers a refusal in the product's shape.</summary>
    /// <param name="type">The error code.</param>
    /// <param name="title">The class of failure.</param>
    /// <param name="status">The HTTP status.</param>
    /// <param name="detail">The localized sentence.</param>
    /// <param name="errors">Field-level messages, if any.</param>
    /// <returns>The response.</returns>
    protected IActionResult Error(
        string type, string title, int status, string detail, Dictionary<string, List<string>>? errors = null)
    {
        return ErrorBody.ToActionResult(new ErrorResponse(type, title, status, detail, errors));
    }

    /// <summary>200 with the value, or the mapped refusal.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The handler's outcome.</param>
    /// <returns>The response.</returns>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsFailure ? MapFailure(result) : Ok(result.Value);
    }

    /// <summary>204, or the mapped refusal.</summary>
    /// <param name="result">The handler's outcome.</param>
    /// <returns>The response.</returns>
    protected IActionResult HandleResult(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsFailure ? MapFailure(result) : NoContent();
    }

    /// <summary>200 with an empty body, or the mapped refusal.</summary>
    /// <param name="result">The handler's outcome.</param>
    /// <returns>The response.</returns>
    protected IActionResult HandleResultOk(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsFailure ? MapFailure(result) : Ok();
    }

    /// <summary>200 with data that did not come from a use case (a health probe, a static list).</summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="data">The data.</param>
    /// <returns>The response.</returns>
    protected IActionResult HandleData<T>(T data)
    {
        return Ok(data);
    }

    /// <summary>201 with a relative location.</summary>
    /// <typeparam name="T">The created resource type.</typeparam>
    /// <param name="relativeUri">Where the resource now lives, relative to the API root.</param>
    /// <param name="value">The resource.</param>
    /// <returns>The response.</returns>
    protected IActionResult CreatedResult<T>(string relativeUri, T value)
    {
        return Created(relativeUri, value);
    }

    /// <summary>A success that is not a plain 200; the refusal path stays identical.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The handler's outcome.</param>
    /// <param name="onSuccess">What to answer with on success.</param>
    /// <returns>The response.</returns>
    protected IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);

        return result.IsSuccess ? onSuccess(result.Value!) : MapFailure(result);
    }

    /// <summary>The file download the handler produced, or the mapped refusal.</summary>
    /// <typeparam name="T">The export DTO type.</typeparam>
    /// <param name="result">The handler's outcome.</param>
    /// <returns>A file response with the DTO's bytes, content type and download name.</returns>
    protected IActionResult FileResult<T>(Result<T> result)
        where T : IFileDownload
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsFailure || result.Value is null)
        {
            return MapFailure(result);
        }

        return File(result.Value.FileContent, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>202 with no body for queued work, or the mapped refusal.</summary>
    /// <param name="result">The handler's outcome.</param>
    /// <returns>The response.</returns>
    protected IActionResult AcceptedResult(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsFailure ? MapFailure(result) : StatusCode(StatusCodes.Status202Accepted);
    }

    /// <summary>Maps a refusal to its status and localizes its sentence by code.</summary>
    /// <remarks>
    /// Protected so that an action needing a non-default success shape routes its failure through
    /// the one canonical mapping instead of choosing a status itself.
    /// </remarks>
    /// <param name="result">A failed outcome.</param>
    /// <returns>The response.</returns>
    protected IActionResult MapFailure(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var code = result.ErrorCode ?? SharedErrorCodes.Unexpected;
        var (status, title) = StatusMapping.For(code);

        // Localized by code, not by the handler's English sentence: the resource file holds the
        // person-facing wording in every language, and the handler's Error string goes to the log.
        var resolver = HttpContext.RequestServices.GetRequiredService<IErrorMessageResolver>();
        return Error(code, title, status, resolver.Resolve(code), result.Errors);
    }
}
