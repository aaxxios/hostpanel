using System.Diagnostics.CodeAnalysis;
using Innovayse.Hostpanel.Shared.Constants;

namespace Innovayse.Hostpanel.API.Auth;

/// <summary>Reads a bearer token from a request's <c>Authorization</c> header, the one place that header is parsed.</summary>
public static class BearerHeader
{
    /// <summary>Reads the bearer token the request presents in its <c>Authorization</c> header.</summary>
    /// <param name="request">The incoming request whose <c>Authorization</c> header is read.</param>
    /// <param name="token">The token after the <c>Bearer </c> prefix, when one is presented.</param>
    /// <returns>True when the header carries a non-empty bearer token.</returns>
    public static bool TryRead(HttpRequest request, [NotNullWhen(true)] out string? token)
    {
        var header = request.Headers.Authorization.FirstOrDefault();
        if (header is null
            || !header.StartsWith(BearerScheme.HeaderPrefix, StringComparison.OrdinalIgnoreCase)
            || header.Length == BearerScheme.HeaderPrefix.Length)
        {
            token = null;
            return false;
        }

        token = header[BearerScheme.HeaderPrefix.Length..];
        return true;
    }
}
