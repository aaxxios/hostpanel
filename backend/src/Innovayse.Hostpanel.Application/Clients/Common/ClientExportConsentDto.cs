using Innovayse.Hostpanel.Application.Clients.Common.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>
/// One consent the client has given or withheld, as the <c>consentHistory</c> section of a client data
/// export lists it. Only the current choice is held; earlier changes are not recorded.
/// </summary>
/// <param name="Purpose">What the consent is for.</param>
/// <param name="Granted">Whether the client agreed.</param>
public sealed record ClientExportConsentDto(ConsentPurpose Purpose, bool Granted);
