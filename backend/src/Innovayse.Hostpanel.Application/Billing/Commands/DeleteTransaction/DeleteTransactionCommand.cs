namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteTransaction;

/// <summary>Command to delete a transaction and reverse any credit adjustments.</summary>
/// <param name="Id">The transaction's primary key.</param>
public sealed record DeleteTransactionCommand(int Id);
