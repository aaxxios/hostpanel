namespace Innovayse.Hostpanel.Domain.Domains.Enums;
/// <summary>
/// Identifies the registrar integration module used for domain operations.
/// Each value maps to a concrete <c>IRegistrarProvider</c> implementation
/// (<c>Innovayse.Hostpanel.Application.Common.Interfaces</c> -- Domain cannot reference it, so
/// this cannot be a resolvable <c>cref</c>).
/// </summary>
public enum RegistrarModule
{
    /// <summary>The name.am registrar module (Armenian TLDs).</summary>
    NameAm = 0,

    /// <summary>The Namecheap registrar module (generic and country-code TLDs).</summary>
    Namecheap = 1,
}
