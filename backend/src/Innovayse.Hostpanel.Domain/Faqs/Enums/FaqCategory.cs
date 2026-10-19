namespace Innovayse.Hostpanel.Domain.Faqs.Enums;

/// <summary>The topic a frequently asked question belongs to; the storefront filters its FAQ by it.</summary>
public enum FaqCategory
{
    /// <summary>Ordering, billing, support and how the platform works.</summary>
    General = 0,

    /// <summary>Hosting plans, domains and servers.</summary>
    Products = 1
}
