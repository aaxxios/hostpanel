using Innovayse.Hostpanel.Domain.Services;

namespace Innovayse.Hostpanel.Application.Provisioning.Common;

/// <summary>The hosting account an operation acts on: the service it belongs to, its server and its panel reference.</summary>
/// <param name="ServiceId">The client service the account belongs to.</param>
/// <param name="ServerId">The server the account lives on, or <see langword="null"/> when none is assigned.</param>
/// <param name="ProvisioningRef">The panel's reference for the account, or <see langword="null"/> when it was never provisioned.</param>
public sealed record HostedAccountTarget(int ServiceId, int? ServerId, string? ProvisioningRef)
{
    /// <summary>Reads the account a service holds.</summary>
    /// <param name="service">The service whose account is acted on.</param>
    /// <returns>The service's account target.</returns>
    public static HostedAccountTarget Of(ClientService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return new HostedAccountTarget(service.Id, service.ServerId, service.ProvisioningRef);
    }
}
