using Innovayse.Hostpanel.Domain.Ssh.Enums;

namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>SSH access summary for one service, as the portal shows it.</summary>
/// <param name="State">Whether keys can be managed and, if not, why.</param>
/// <param name="Hostname">Hostname to connect to; <see langword="null"/> unless the server is configured.</param>
/// <param name="Port">Port to connect to; <see langword="null"/> unless the server is configured.</param>
/// <param name="Username">The customer's own hosting account name; <see langword="null"/> unless configured.</param>
/// <param name="ConnectCommand">A ready-to-copy <c>ssh</c> command; <see langword="null"/> unless keys can be managed.</param>
/// <param name="MaxKeys">How many keys the service may hold.</param>
public sealed record SshAccessDto(
    SshAccessState State,
    string? Hostname,
    int? Port,
    string? Username,
    string? ConnectCommand,
    int MaxKeys);
