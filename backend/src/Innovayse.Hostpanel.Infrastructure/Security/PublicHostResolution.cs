using System.Net;
using Innovayse.Hostpanel.Infrastructure.Security.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Security;

/// <summary>The outcome of <see cref="PublicHostGuard.ResolveAsync"/>.</summary>
/// <param name="Verdict">What was decided about the name.</param>
/// <param name="Addresses">The vetted addresses to dial; empty unless <paramref name="Verdict"/> is <see cref="PublicHostVerdict.Public"/>.</param>
public sealed record PublicHostResolution(PublicHostVerdict Verdict, IReadOnlyList<IPAddress> Addresses)
{
    /// <summary>The name must not be dialled.</summary>
    public static PublicHostResolution Refused { get; } = new(PublicHostVerdict.Refused, []);

    /// <summary>The name is acceptable but resolved to nothing.</summary>
    public static PublicHostResolution Unresolved { get; } = new(PublicHostVerdict.Unresolved, []);
}
