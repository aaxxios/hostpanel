namespace Innovayse.Hostpanel.Infrastructure.Security.Enums;

/// <summary>What <see cref="PublicHostGuard"/> decided about a client-supplied host name.</summary>
public enum PublicHostVerdict
{
    /// <summary>The name is not a public host, or it resolves to an internal address. Nothing may be dialled.</summary>
    Refused,

    /// <summary>The name is well formed but resolves to no address (a registered domain with no web host, or a slow resolver).</summary>
    Unresolved,

    /// <summary>The name resolves, and every address it resolves to is public.</summary>
    Public,
}
