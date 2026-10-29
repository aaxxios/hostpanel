namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>An amount to move onto or off a client's credit balance.</summary>
/// <param name="ClientId">The client whose balance changes.</param>
/// <param name="Amount">The positive amount, in the currency the balance is kept in.</param>
public sealed record ClientCreditChange(int ClientId, decimal Amount);
