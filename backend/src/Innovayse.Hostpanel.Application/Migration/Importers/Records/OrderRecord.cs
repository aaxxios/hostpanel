namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One order as the source plugin describes it, with its line items.</summary>
/// <param name="ClientEmail">Email of the owning client in the source; the importer finds the local client by it and skips the record when none matches.</param>
/// <param name="OrderNumber">The order number in the source; the duplicate check.</param>
/// <param name="PaymentMethod">The source's gateway name for how it is paid; mapped to a local gateway where one matches.</param>
/// <param name="Status">The source's status word for the record, mapped to a local status by the importer's source-value table.</param>
/// <param name="Amount">Total amount of the record, in the client's currency.</param>
/// <param name="CreatedAt">When the source created the record (UTC); null when the source holds no date.</param>
/// <param name="Items">The line items: the services, service addons and domains the source ordered under this order, in that order.</param>
internal sealed record OrderRecord(string ClientEmail, string OrderNumber, string PaymentMethod, string Status, decimal Amount, DateTimeOffset? CreatedAt, List<OrderItemRecord>? Items);
