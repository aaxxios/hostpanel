namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>A row of the items table on an invoice PDF.</summary>
/// <param name="Description">Charge description; wraps within its column.</param>
/// <param name="Quantity">Number of units.</param>
/// <param name="UnitPrice">Price per unit.</param>
/// <param name="Amount">Line total.</param>
public sealed record InvoicePdfLine(string Description, int Quantity, decimal UnitPrice, decimal Amount);
