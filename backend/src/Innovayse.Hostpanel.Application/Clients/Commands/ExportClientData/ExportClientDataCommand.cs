namespace Innovayse.Hostpanel.Application.Clients.Commands.ExportClientData;

/// <summary>
/// Command for the admin "export client data" download: returns the sections of a client's record
/// that were explicitly asked for, and nothing else.
/// </summary>
/// <param name="ClientId">The client's primary key.</param>
/// <param name="Fields">
/// The section keys to include, matched case-insensitively: <c>profileData</c>, <c>domains</c>,
/// <c>productsServices</c>, <c>invoices</c>, <c>transactions</c>, <c>quotes</c>,
/// <c>billableItems</c>, <c>contacts</c>, <c>users</c>, <c>invitations</c>, <c>tickets</c>, <c>emails</c>,
/// <c>notes</c>, <c>activityLog</c>, <c>consentHistory</c>, <c>payMethods</c> (see
/// <see cref="Innovayse.Hostpanel.Application.Clients.Common.ClientExportSections"/>). A key that is absent
/// leaves its section out of the payload entirely rather than emitting it empty; a key that is not one of
/// these is refused with 400.
/// </param>
public sealed record ExportClientDataCommand(int ClientId, IReadOnlyList<string> Fields);
