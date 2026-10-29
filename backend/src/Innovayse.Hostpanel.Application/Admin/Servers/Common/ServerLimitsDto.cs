namespace Innovayse.Hostpanel.Application.Admin.Servers.Common;

/// <summary>How many accounts a provisioning server may take and whether it is taking new ones.</summary>
/// <param name="MaxAccounts">Account capacity limit, or null for unlimited.</param>
/// <param name="IsDefault">Whether this is the default server for its module type.</param>
/// <param name="IsDisabled">Whether new accounts are blocked from this server.</param>
public sealed record ServerLimitsDto(int? MaxAccounts, bool IsDefault, bool IsDisabled);
