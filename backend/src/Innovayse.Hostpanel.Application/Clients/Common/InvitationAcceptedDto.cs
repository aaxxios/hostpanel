namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>The acknowledgement the accept-invitation endpoint answers with once the caller is linked to the client.</summary>
/// <param name="Success">Always true; a refusal is an error response instead.</param>
public sealed record InvitationAcceptedDto(bool Success);
