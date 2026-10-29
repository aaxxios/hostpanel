namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Represents a single line item submitted with <see cref="Innovayse.Hostpanel.Application.Billing.Commands.CreateInvoice.CreateInvoiceCommand"/>.</summary>
/// <param name="Description">Human-readable charge description.</param>
/// <param name="UnitPrice">Price per unit (≥ 0).</param>
/// <param name="Quantity">Number of units (≥ 1).</param>
public sealed record CreateInvoiceItemDto(string Description, decimal UnitPrice, int Quantity);
