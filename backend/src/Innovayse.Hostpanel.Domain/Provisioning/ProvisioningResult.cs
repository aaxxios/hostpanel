namespace Innovayse.Hostpanel.Domain.Provisioning;

/// <summary>
/// Result of a provisioning operation returned by the <c>IProvisioningProvider</c> port
/// (<c>Innovayse.Hostpanel.Application.Common.Interfaces</c> -- Domain cannot reference it, so
/// this cannot be a resolvable <c>cref</c>).
/// </summary>
/// <param name="Success">Indicates whether the operation completed successfully.</param>
/// <param name="ProvisioningRef">
/// Provider-assigned reference identifier for the provisioned account; null on failure.
/// </param>
/// <param name="ErrorMessage">Human-readable error description; null on success.</param>
public record ProvisioningResult(bool Success, string? ProvisioningRef, string? ErrorMessage);
