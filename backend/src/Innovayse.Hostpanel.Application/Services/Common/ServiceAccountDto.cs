namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>The hosting account a service was provisioned as.</summary>
/// <param name="DedicatedIp">Dedicated IP address.</param>
/// <param name="Username">Hosting account username.</param>
/// <param name="Password">Hosting account password.</param>
/// <param name="ProvisioningRef">External provisioning reference (e.g., hosting account name).</param>
public sealed record ServiceAccountDto(string? DedicatedIp, string? Username, string? Password, string? ProvisioningRef);
