namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>One side of an invoice PDF, the seller or the client, as the lines printed under its name.</summary>
/// <param name="Name">Printed in bold as the first line.</param>
/// <param name="Lines">Address and identifier lines below the name, empty values already removed.</param>
public sealed record InvoicePdfParty(string Name, IReadOnlyList<string> Lines);
