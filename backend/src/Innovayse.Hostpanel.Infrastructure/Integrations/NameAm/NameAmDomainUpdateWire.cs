namespace Innovayse.Hostpanel.Infrastructure.Integrations.NameAm;

/// <summary>
/// The body of Name.am's <c>PUT /client/domains/{name}</c>. Each call changes one thing, so every
/// member is optional and only the one being changed is sent.
/// </summary>
/// <param name="WhoIsPrivacyStatus">Switches WHOIS privacy on or off.</param>
/// <param name="TransferLock">Locks or unlocks the domain against transfer.</param>
/// <param name="NameServers">Replaces the nameservers.</param>
/// <param name="Records">Replaces the zone with the full record set.</param>
/// <param name="RegistrantContacts">Replaces the registrant contact.</param>
/// <param name="AdministrativeContacts">Replaces the administrative contact.</param>
/// <param name="TechnicalContacts">Replaces the technical contact.</param>
/// <param name="BillingContacts">Replaces the billing contact.</param>
internal sealed record NameAmDomainUpdateWire(
    bool? WhoIsPrivacyStatus = null,
    bool? TransferLock = null,
    IReadOnlyList<NameAmNameServerWire>? NameServers = null,
    IReadOnlyList<NameAmDnsRecordWire>? Records = null,
    NameAmContactWire? RegistrantContacts = null,
    NameAmContactWire? AdministrativeContacts = null,
    NameAmContactWire? TechnicalContacts = null,
    NameAmContactWire? BillingContacts = null);
