namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One quote as the source plugin describes it, with its line items.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="Subject">The subject.</param>
/// <param name="Stage">The stage.</param>
/// <param name="ExpiryDate">The expiry date.</param>
/// <param name="ProposalText">The proposal text.</param>
/// <param name="CustomerNotes">The customer notes.</param>
/// <param name="Items">The items entries.</param>
/// <param name="CreatedAt">The created at.</param>
internal sealed record QuoteRecord(string ClientEmail, string Subject, string Stage, DateTimeOffset ExpiryDate, string? ProposalText, string? CustomerNotes, List<QuoteItemRecord> Items, DateTimeOffset CreatedAt);
