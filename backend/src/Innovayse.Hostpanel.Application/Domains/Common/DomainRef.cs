namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Identifies a domain at the registrar: its name and the registrar's own reference for it.
/// </summary>
/// <param name="Name">Fully-qualified domain name.</param>
/// <param name="RegistrarRef">Registrar-assigned reference for the domain; empty when none is held yet.</param>
public sealed record DomainRef(string Name, string RegistrarRef);
