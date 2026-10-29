namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One client exactly as the source plugin sends it: every field flat, in the plugin's own shape.</summary>
internal sealed record ClientRecordWire
{
    /// <summary>The client's email.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Given name as stored in the source.</summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Family name as stored in the source.</summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>Company name; null or empty when the source holds none.</summary>
    public string? Company { get; init; }

    /// <summary>Phone number, free text.</summary>
    public string? Phone { get; init; }

    /// <summary>Street address line, free text.</summary>
    public string? Address { get; init; }

    /// <summary>City of the postal address.</summary>
    public string? City { get; init; }

    /// <summary>State or region of the postal address.</summary>
    public string? State { get; init; }

    /// <summary>Postal code of the address.</summary>
    public string? PostCode { get; init; }

    /// <summary>Two-letter ISO country code of the address.</summary>
    public string? Country { get; init; }

    /// <summary>The source's client status word.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>ISO 4217 code of the currency the client is billed in.</summary>
    public string? Currency { get; init; }

    /// <summary>Whether the client is exempt from tax on invoices.</summary>
    public bool TaxExempt { get; init; }

    /// <summary>Credit the client holds in the source.</summary>
    public decimal CreditBalance { get; init; }

    /// <summary>When the client signed up in the source.</summary>
    public DateTimeOffset? SignupDate { get; init; }

    /// <summary>When the client last signed in to the source; null when never.</summary>
    public DateTimeOffset? LastLogin { get; init; }

    /// <summary>Groups the flat fields by what they describe.</summary>
    /// <returns>The grouped client record.</returns>
    public ClientRecord ToRecord() => new(
        Email,
        FirstName,
        LastName,
        Company,
        Phone,
        new ClientAddressRecord(Address, City, State, PostCode, Country),
        new ClientBillingRecord(Currency, TaxExempt, CreditBalance),
        Status,
        SignupDate,
        LastLogin);
}
