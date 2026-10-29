namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One domain as the source plugin describes it.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="DomainName">Fully qualified domain name; also the duplicate check.</param>
/// <param name="RegisteredAt">When the domain was registered; null when the source holds no date, and the record is then skipped.</param>
/// <param name="ExpiresAt">When the registration expires at the registry; null when the source holds no date, and the record is then skipped.</param>
/// <param name="NextDueDate">When the next renewal invoice falls due.</param>
/// <param name="Registrar">The source's registrar module name; kept for reference.</param>
/// <param name="Nameservers">Nameserver host names, in order; empty when the source holds none.</param>
/// <param name="Status">The source's status word for the record, mapped to a local status by the importer's source-value table.</param>
/// <param name="AutoRenew">Whether the domain renews automatically.</param>
/// <param name="RecurringAmount">Amount billed each renewal, in the client's currency.</param>
/// <param name="FirstPaymentAmount">Amount billed on the first invoice, in the client's currency.</param>
/// <param name="PaymentMethod">The source's gateway name for how it is paid; mapped to a local gateway where one matches.</param>
/// <param name="SubscriptionId">The gateway subscription reference; null when the source has none.</param>
/// <param name="AdminNotes">Staff-only notes, carried over as they are.</param>
internal sealed record DomainRecord(
    string ClientEmail, string DomainName,
    DateTimeOffset? RegisteredAt, DateTimeOffset? ExpiresAt,
    DateTimeOffset? NextDueDate, string? Registrar, List<string>? Nameservers,
    string? Status, bool AutoRenew, decimal RecurringAmount, decimal FirstPaymentAmount,
    string? PaymentMethod, string? SubscriptionId, string? AdminNotes);
