namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One line item on an <see cref="InvoiceRecord"/>.</summary>
/// <param name="Description">The line text shown on the invoice.</param>
/// <param name="Amount">Line amount, in the invoice currency.</param>
/// <param name="Quantity">How many units the line bills.</param>
internal sealed record InvoiceItemRecord(string Description, decimal Amount, int Quantity);
