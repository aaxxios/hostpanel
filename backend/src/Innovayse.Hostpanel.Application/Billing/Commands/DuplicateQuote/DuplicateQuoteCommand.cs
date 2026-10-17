namespace Innovayse.Hostpanel.Application.Billing.Commands.DuplicateQuote;

/// <summary>Command to duplicate a quote as a new draft.</summary>
/// <param name="QuoteId">The quote to duplicate, taken from the route.</param>
public sealed record DuplicateQuoteCommand(int QuoteId);
