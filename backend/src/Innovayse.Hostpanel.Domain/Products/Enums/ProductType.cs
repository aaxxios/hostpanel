namespace Innovayse.Hostpanel.Domain.Products.Enums;
/// <summary>Classifies a product by the type of service it represents.</summary>
public enum ProductType
{
    /// <summary>Shared web hosting plan.</summary>
    SharedHosting = 0,

    /// <summary>Virtual private server.</summary>
    Vps = 1,

    /// <summary>Dedicated server.</summary>
    Dedicated = 2,

    /// <summary>Domain name registration.</summary>
    Domain = 3,

    /// <summary>SSL certificate.</summary>
    Ssl = 4,

    /// <summary>Other or custom service.</summary>
    Other = 5,
}
