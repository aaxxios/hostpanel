namespace Innovayse.Hostpanel.Application.Admin.Servers.Common;

/// <summary>Which secrets are stored for a provisioning server; the secrets themselves are never returned.</summary>
/// <param name="HasPassword">Whether an API password is stored.</param>
/// <param name="HasApiToken">Whether an API token is stored.</param>
/// <param name="HasAccessHash">Whether an access hash is stored.</param>
public sealed record ServerCredentialStatusDto(bool HasPassword, bool HasApiToken, bool HasAccessHash);
