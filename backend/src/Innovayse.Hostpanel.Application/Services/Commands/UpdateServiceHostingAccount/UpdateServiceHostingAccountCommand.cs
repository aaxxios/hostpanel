namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceHostingAccount;

/// <summary>
/// Staff correct the record of the hosting account a service lives on: server, domain, panel
/// reference, dedicated address and credentials, as saved from the hosting account section of the
/// service page. Nothing is changed on the panel itself.
/// </summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="ServerId">The server the account lives on, or <see langword="null"/> for none.</param>
/// <param name="Domain">The account's primary domain, or <see langword="null"/> to clear it.</param>
/// <param name="ProvisioningRef">The account's reference on the panel, or <see langword="null"/> to clear it.</param>
/// <param name="DedicatedIp">The dedicated IP address, or <see langword="null"/> to clear it.</param>
/// <param name="Username">The account's username on the panel, or <see langword="null"/> to clear it.</param>
/// <param name="Password">The account's password on the panel, or <see langword="null"/> to clear it.</param>
public sealed record UpdateServiceHostingAccountCommand(
    int ServiceId,
    int? ServerId,
    string? Domain,
    string? ProvisioningRef,
    string? DedicatedIp,
    string? Username,
    string? Password);
