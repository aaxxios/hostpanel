namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>One nameserver in Name.am's JSON.</summary>
/// <param name="Hostname">The nameserver host name.</param>
internal sealed record NameAmNameServerWire(string Hostname);
