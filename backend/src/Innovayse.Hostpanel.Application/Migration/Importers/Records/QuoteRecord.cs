namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One quote as the source plugin describes it, with its line items.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="Subject">The subject line; together with the client email it is the duplicate check.</param>
/// <param name="Stage">The source's quote stage word, mapped to a local stage.</param>
/// <param name="ExpiryDate">Date the quote stops being valid; null when the source holds no date, and the record is then skipped.</param>
/// <param name="ProposalText">Text shown to the client above the lines.</param>
/// <param name="CustomerNotes">Notes shown to the client below the lines.</param>
/// <param name="Items">The line items, in source order.</param>
/// <param name="CreatedAt">When the source created the record (UTC); null when the source holds no date.</param>
internal sealed record QuoteRecord(string ClientEmail, string Subject, string Stage, DateTimeOffset? ExpiryDate, string? ProposalText, string? CustomerNotes, List<QuoteItemRecord> Items, DateTimeOffset? CreatedAt);
