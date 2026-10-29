namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One line item on an <see cref="OrderRecord"/>.</summary>
/// <param name="ProductId">The source id of the ordered product, resolved to the local product through <see cref="MigrationProductLookup.LoadSourceProductMapAsync"/>; null for a line that is no product (a domain or a service addon), which is not imported as an order item.</param>
/// <param name="ProductName">The product's name in the source (the addon's name, or the domain registration or transfer for a domain line); kept on the order item as the name it was ordered under, never used to find a product.</param>
/// <param name="BillingCycle">The source's billing cycle word (monthly, annually and so on), mapped to a local cycle.</param>
/// <param name="FirstPaymentAmount">Amount billed on the first invoice, in the client's currency.</param>
/// <param name="RecurringAmount">Amount billed each renewal, in the client's currency.</param>
/// <param name="Domain">The domain name the line or service is for; null or empty when none.</param>
/// <param name="Hostname">Server host name ordered with the line; null when none.</param>
internal sealed record OrderItemRecord(int? ProductId, string ProductName, string BillingCycle, decimal FirstPaymentAmount, decimal RecurringAmount, string? Domain, string? Hostname);
