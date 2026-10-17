namespace Innovayse.Hostpanel.Application.Auth.Queries.GetMyAccount;

/// <summary>
/// Query for the authenticated caller's email and local roles. Carries no subject: who is asking
/// is resolved inside the handler from the credential.
/// </summary>
public sealed record GetMyAccountQuery();
