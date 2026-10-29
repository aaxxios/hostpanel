namespace Innovayse.Hostpanel.Domain.Clients;

/// <summary>
/// What was captured about the device and network of a customer who registered themselves, kept on
/// the <see cref="Client"/> for fraud review and support. Absent for accounts an admin created.
/// </summary>
/// <param name="IpAddress">The IP address the registration came from.</param>
/// <param name="UserAgent">The raw browser or device user-agent.</param>
/// <param name="DeviceType">The device type parsed from the user-agent (Desktop, Mobile, Tablet).</param>
/// <param name="OperatingSystem">The operating system parsed from the user-agent.</param>
/// <param name="Browser">The browser name and version parsed from the user-agent.</param>
public sealed record RegistrationInfo(
    string? IpAddress,
    string? UserAgent,
    string? DeviceType,
    string? OperatingSystem,
    string? Browser);
