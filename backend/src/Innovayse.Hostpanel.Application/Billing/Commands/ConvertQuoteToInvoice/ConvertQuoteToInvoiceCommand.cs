namespace Innovayse.Hostpanel.Application.Billing.Commands.ConvertQuoteToInvoice;

/// <summary>Command to convert a quote into a draft invoice.</summary>
/// <param name="QuoteId">The quote to convert, taken from the route.</param>
public sealed record ConvertQuoteToInvoiceCommand(int QuoteId);
