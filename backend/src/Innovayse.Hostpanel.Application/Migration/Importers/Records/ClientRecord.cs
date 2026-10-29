using System.Text.Json.Serialization;

namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>
/// One client as the source plugin describes it. The plugin sends the fields flat; the
/// <see cref="ClientRecordConverter"/> reads them into this grouped shape.
/// </summary>
/// <param name="Email">The client's email; the duplicate check and the key other records link to the client by.</param>
/// <param name="FirstName">Given name as stored in the source.</param>
/// <param name="LastName">Family name as stored in the source.</param>
/// <param name="Company">Company name; null or empty when the source holds none.</param>
/// <param name="Phone">Phone number, free text as stored in the source.</param>
/// <param name="Address">The postal address.</param>
/// <param name="Billing">The currency, tax setting and credit the client is billed with.</param>
/// <param name="Status">The source's client status (active, inactive, closed), mapped to a local status.</param>
/// <param name="SignupDate">When the client signed up in the source.</param>
/// <param name="LastLogin">When the client last signed in to the source; null when never.</param>
[JsonConverter(typeof(ClientRecordConverter))]
internal sealed record ClientRecord(
    string Email,
    string FirstName,
    string LastName,
    string? Company,
    string? Phone,
    ClientAddressRecord Address,
    ClientBillingRecord Billing,
    string Status,
    DateTimeOffset? SignupDate,
    DateTimeOffset? LastLogin);
