namespace Innovayse.Hostpanel.Domain.Billing;

/// <summary>The data of one quote line that an invoice raised from the quote bills.</summary>
/// <param name="Description">The line description.</param>
/// <param name="UnitPrice">The price of one unit, in the quote's currency.</param>
/// <param name="Quantity">The number of units.</param>
public sealed record QuoteInvoiceLine(string Description, decimal UnitPrice, int Quantity);
