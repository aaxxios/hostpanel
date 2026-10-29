namespace Innovayse.Hostpanel.Application.Admin.Servers.Common;

/// <summary>Where a provisioning server is hosted and what it costs.</summary>
/// <param name="Datacenter">Datacenter or NOC provider name, or null.</param>
/// <param name="ServerStatusAddress">URL to the server status page folder, or null.</param>
/// <param name="MonthlyCost">Monthly cost in USD.</param>
public sealed record ServerHostingDto(string? Datacenter, string? ServerStatusAddress, decimal MonthlyCost);
