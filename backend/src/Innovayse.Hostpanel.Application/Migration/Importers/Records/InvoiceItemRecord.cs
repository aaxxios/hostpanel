namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One line item on an <see cref="InvoiceRecord"/>.</summary>
/// <param name="Description">The description.</param>
/// <param name="Amount">The amount.</param>
/// <param name="Quantity">The quantity.</param>
internal sealed record InvoiceItemRecord(string Description, decimal Amount, int Quantity);
