namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>One item of the cart Name.am's <c>POST /client/carts/purchase</c> buys.</summary>
/// <param name="Name">The suffix the item is bought under.</param>
/// <param name="Type">The kind of purchase: <c>domain_registration</c>, <c>domain_transfer</c> or <c>domain_renew</c>.</param>
/// <param name="Domain">The fully-qualified domain name.</param>
/// <param name="Plan">The plan the item is bought on.</param>
/// <param name="TransferCode">The EPP code of a transfer; absent for the other kinds.</param>
/// <param name="RegistrantContacts">The registrant contact; absent for a renewal.</param>
/// <param name="AdministrativeContacts">The administrative contact; absent for a renewal.</param>
/// <param name="TechnicalContacts">The technical contact; absent for a renewal.</param>
/// <param name="BillingContacts">The billing contact; absent for a renewal.</param>
/// <param name="NameServers">The nameservers of a registration; absent when the registrar defaults apply.</param>
internal sealed record NameAmPurchaseItemWire(
    string Name,
    string Type,
    string Domain,
    NameAmPlanWire Plan,
    string? TransferCode = null,
    NameAmContactWire? RegistrantContacts = null,
    NameAmContactWire? AdministrativeContacts = null,
    NameAmContactWire? TechnicalContacts = null,
    NameAmContactWire? BillingContacts = null,
    IReadOnlyList<NameAmNameServerWire>? NameServers = null);
