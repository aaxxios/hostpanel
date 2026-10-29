namespace Innovayse.Hostpanel.Application.Demo.Common.Enums;

/// <summary>Which side of the product a published demo account signs in to. Serialized as a string.</summary>
public enum DemoAccountRole
{
    /// <summary>The staff account for the admin panel.</summary>
    Admin,

    /// <summary>The customer account for the client portal.</summary>
    Client,
}
