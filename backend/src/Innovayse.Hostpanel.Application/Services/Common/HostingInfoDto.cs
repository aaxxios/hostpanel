namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>The live nameservers and certificate of a hosted domain.</summary>
/// <param name="Nameservers">The nameservers the domain points at, sorted; empty when none could be read.</param>
/// <param name="Ssl">The certificate the domain presents, or <see langword="null"/> when none could be read.</param>
public sealed record HostingInfoDto(IReadOnlyList<string> Nameservers, HostingSslDto? Ssl)
{
    /// <summary>The answer for a domain nothing may be read about: no nameservers and no certificate.</summary>
    public static HostingInfoDto Empty { get; } = new([], null);
}
