namespace Innovayse.Hostpanel.Domain.Clients.Enums;

/// <summary>Classification of an additional contact on a client account.</summary>
public enum ContactType
{
    /// <summary>Receives billing and invoice emails.</summary>
    Billing = 0,

    /// <summary>Receives technical and service notifications.</summary>
    Technical = 1,

    /// <summary>General purpose contact.</summary>
    General = 2,
}
