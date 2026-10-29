namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>A client's credit balance after a change was staged.</summary>
/// <param name="ClientId">The client the balance belongs to.</param>
/// <param name="Balance">The balance once the staged change is saved.</param>
public sealed record ClientCreditBalance(int ClientId, decimal Balance);
