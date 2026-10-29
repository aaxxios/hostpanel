using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// The httpOnly cookie a local-mode browser session rides on, and the two operations that
/// write it.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists.</b> Under <c>Auth:Mode=sso</c> the API performs the OIDC exchange and the
/// browser holds an opaque httpOnly cookie, so no script on the page can read a credential. Local
/// mode needs the same property: a bearer JWT in <c>sessionStorage</c> is readable by any injected
/// script, and memory-only storage logs the operator out on every refresh.
/// </para>
/// <para>
/// The cookie is the same answer as the external sign-on path's, applied to the credential local mode
/// already mints: <c>IJwtService</c> issues the same 15-minute JWT under the same <c>LocalJwt</c>
/// validation parameters, and the browser keeps it in a cookie it cannot read and does not
/// forget on a refresh.
/// </para>
/// <para>
/// <b>It does not touch the external sign-on path.</b> This cookie is written only by
/// <c>BaseApiController.HandleSession</c> for the local sign-in routes, which answer 404 unless the deployment is in local
/// mode, and read only by the bearer scheme registered in <c>Program.cs</c>'s local branch. The
/// external sign-on branch registers the sign-on package's own cookie handler and is not involved.
/// </para>
/// <para>
/// <b>The bearer header still works.</b> It has to: the client portal's Nuxt server calls this
/// API machine-to-machine with an <c>Authorization</c> header and has no browser to hold a
/// cookie. The cookie is read only when no such header was sent.
/// </para>
/// </remarks>
public static class LocalSessionCookie
{
    /// <summary>
    /// Name of the cookie carrying the local-mode access token.
    /// </summary>
    /// <remarks>
    /// The <c>__Host-</c> prefix is deliberately <b>not</b> used. It would be stronger, a
    /// browser refuses such a cookie unless it is Secure, host-only and path <c>/</c>, but it
    /// also makes the cookie impossible to set over plain HTTP, which a Development host on
    /// <c>http://localhost</c> relies on. <see cref="Issue"/> sets <c>Secure</c> on every
    /// request outside Development and on any HTTPS request, which gives a real deployment the
    /// same protection.
    /// </remarks>
    public const string Name = "hostpanel_session";

    /// <summary>
    /// Writes the access token into the session cookie on the response.
    /// </summary>
    /// <param name="context">The HTTP context of the request being answered.</param>
    /// <param name="accessToken">The JWT minted for this sign-in.</param>
    public static void Issue(HttpContext context, string accessToken) =>
        context.Response.Cookies.Append(Name, accessToken, BuildOptions(context));

    /// <summary>
    /// Removes the session cookie, ending the browser's half of the session.
    /// </summary>
    /// <param name="context">The HTTP context of the request being answered.</param>
    /// <remarks>
    /// The attributes have to match the ones the cookie was written with or the browser keeps
    /// the original, which is why this shares <see cref="BuildOptions"/> with
    /// <see cref="Issue"/> rather than spelling out a second set.
    /// </remarks>
    public static void Clear(HttpContext context) =>
        context.Response.Cookies.Delete(Name, BuildOptions(context));

    /// <summary>
    /// The attributes the session cookie is written and deleted with.
    /// </summary>
    /// <param name="context">The HTTP context of the request being answered.</param>
    /// <returns>Cookie options for <see cref="Name"/>.</returns>
    /// <remarks>
    /// <para>
    /// <c>HttpOnly</c> is the whole point: it is what a script on the page cannot read.
    /// </para>
    /// <para>
    /// <c>SameSite=Strict</c>, not Lax. Nothing navigates into the admin panel from another
    /// site, there is no cross-site sign-in redirect on this path, which is precisely the
    /// thing Lax exists to accommodate, so Strict costs nothing and is the first of the two
    /// defences against a cross-site request riding this cookie.
    /// </para>
    /// <para>
    /// <c>Secure</c> always outside Development. It does not follow a forwarded scheme, because a
    /// header is only as trustworthy as the proxy configuration behind it, and a misconfigured
    /// chain must not send a deployed credential in the clear. Only a Development host, a laptop
    /// over plain HTTP, gets a cookie without it, and only on a plain-HTTP request.
    /// </para>
    /// <para>
    /// No <c>Expires</c> or <c>MaxAge</c>: a session cookie, gone when the browser closes. The
    /// token inside it expires in fifteen minutes regardless, so a longer-lived cookie would
    /// only mean carrying something already dead.
    /// </para>
    /// </remarks>
    private static CookieOptions BuildOptions(HttpContext context) => new()
    {
        HttpOnly = true,
        Secure = context.Request.IsHttps
            || !context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/",
    };
}
