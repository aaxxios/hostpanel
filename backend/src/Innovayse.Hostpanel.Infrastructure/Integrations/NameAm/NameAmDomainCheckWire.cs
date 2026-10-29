namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>One name in the body of Name.am's <c>POST /client/domains/check</c>.</summary>
/// <param name="Tld">The suffix after the first label.</param>
/// <param name="Domain">The fully-qualified domain name asked about.</param>
internal sealed record NameAmDomainCheckWire(string Tld, string Domain);
