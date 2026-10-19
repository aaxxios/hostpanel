namespace Innovayse.Hostpanel.Application.Common.Settings;

/// <summary>
/// The contact addresses of the support departments created on a fresh install. Bound from the
/// "DefaultDepartments" section. The defaults are the reserved <c>example.com</c> domain so a
/// tier that forgets to override them cannot mail a real third party; an operator edits the
/// departments in the admin UI afterwards, and these values matter only on the first start.
/// </summary>
public sealed class DefaultDepartmentsOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "DefaultDepartments";

    /// <summary>Address of the technical support department. Single mailbox.</summary>
    public string SupportEmail { get; set; } = "support@example.com";

    /// <summary>Address of the billing department. Single mailbox.</summary>
    public string BillingEmail { get; set; } = "billing@example.com";

    /// <summary>Address of the sales department. Single mailbox.</summary>
    public string SalesEmail { get; set; } = "sales@example.com";

    /// <summary>Address of the general enquiries department. Single mailbox.</summary>
    public string GeneralEmail { get; set; } = "hello@example.com";

    /// <summary>Whether every address is non-empty and shaped like a mailbox.</summary>
    public bool IsValid =>
        IsMailbox(SupportEmail) && IsMailbox(BillingEmail) && IsMailbox(SalesEmail) && IsMailbox(GeneralEmail);

    /// <summary>Cheap shape check: one <c>@</c> with text on both sides.</summary>
    /// <param name="value">The address to check.</param>
    /// <returns>Whether it looks like a mailbox.</returns>
    private static bool IsMailbox(string value)
    {
        var at = value.IndexOf('@');
        return at > 0 && at < value.Length - 1 && value.IndexOf('@', at + 1) < 0;
    }
}
