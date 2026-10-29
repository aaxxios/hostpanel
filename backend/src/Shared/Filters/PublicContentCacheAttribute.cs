using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Innovayse.Hostpanel.Shared.Filters;

/// <summary>
/// Lets a browser or a shared cache reuse a successful public content list for a short time, so the
/// storefront's repeated reads of the same unchanging list do not each reach the database.
/// </summary>
/// <remarks>
/// <para>
/// There is no output cache in this host, so the lifetime is stated once, here, as a
/// <c>Cache-Control</c> header rather than repeated per action. The window is short (<see cref="MaxAgeSeconds"/>
/// seconds) because an operator who edits a list expects the storefront to follow within a minute.
/// </para>
/// <para>
/// The answer depends on the <c>X-Language</c> header (the translation) and on who is asking (some
/// lists, such as slides, are narrowed by the caller), so <c>Vary</c> names both headers. A response
/// computed for a signed-in caller is marked <c>private</c>, so a shared cache never serves it to an
/// anonymous visitor. Only 200 answers are marked; a refusal is never cached.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PublicContentCacheAttribute : ActionFilterAttribute
{
    /// <summary>How long, in seconds, a cached list may be reused.</summary>
    public const int MaxAgeSeconds = 60;

    /// <summary>Marks a successful result as cacheable for <see cref="MaxAgeSeconds"/> seconds.</summary>
    /// <param name="context">The result about to be written.</param>
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Result is not ObjectResult { StatusCode: null or StatusCodes.Status200OK })
        {
            return;
        }

        var scope = context.HttpContext.User.Identity?.IsAuthenticated == true ? "private" : "public";
        var headers = context.HttpContext.Response.Headers;
        headers.CacheControl = $"{scope}, max-age={MaxAgeSeconds}";
        headers.Vary = new[] { HeaderNames.Authorization, "X-Language" };
    }
}
