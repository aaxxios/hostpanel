namespace Innovayse.Hostpanel.Domain.Auth.Enums;

/// <summary>
/// The roles a subject can hold in this product. Stored and put on claims by name, so the
/// member names are the persisted values and must not be renamed.
/// </summary>
public enum Role
{
    /// <summary>Full system administrator with unrestricted access.</summary>
    Admin = 0,

    /// <summary>Reseller who can manage their own clients and services.</summary>
    Reseller = 1,

    /// <summary>End client with access to their own portal only.</summary>
    Client = 2,
}
