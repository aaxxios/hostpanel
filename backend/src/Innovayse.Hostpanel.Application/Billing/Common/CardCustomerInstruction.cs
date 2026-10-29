namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Names the card gateway customer whose saved payment methods are listed.</summary>
/// <param name="CustomerId">The card gateway customer ID.</param>
public sealed record CardCustomerInstruction(string CustomerId);
