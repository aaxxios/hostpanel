namespace Innovayse.Hostpanel.Application.Billing.Commands.RaiseClientRenewalInvoices;

/// <summary>
/// Raises the renewal invoice(s) one client is due. Its own message so each client runs in its own
/// Wolverine scope, a fresh <c>DbContext</c>, and one client's failure cannot leak tracked
/// entities into the next client's save.
/// </summary>
/// <param name="ClientId">The client.</param>
public sealed record RaiseClientRenewalInvoicesCommand(int ClientId);
