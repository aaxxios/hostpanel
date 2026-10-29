namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// The invoice a renewal reminder is about and the email it goes to.
/// </summary>
/// <param name="Email">The recipient's email.</param>
/// <param name="InvoiceId">The renewal invoice's id.</param>
public sealed record RenewalReminderTarget(string Email, int InvoiceId);
