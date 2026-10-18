namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteQuote;

/// <summary>Command to permanently delete a quote.</summary>
/// <param name="QuoteId">The quote to delete, taken from the route.</param>
public sealed record DeleteQuoteCommand(int QuoteId);
