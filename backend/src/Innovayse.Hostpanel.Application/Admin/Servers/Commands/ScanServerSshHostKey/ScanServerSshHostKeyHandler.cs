using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.ScanServerSshHostKey;

/// <summary>
/// Handles <see cref="ScanServerSshHostKeyCommand"/>. The result is shown, never pinned: the
/// administrator compares it with what the installer printed on the server itself, and pins it by
/// saving the settings. A scan alone cannot tell a real server from an interception.
/// </summary>
public sealed class ScanServerSshHostKeyHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Host key scanner.</summary>
    private readonly ISshHostKeyScanner _scanner;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="scanner">Host key scanner.</param>
    public ScanServerSshHostKeyHandler(IHostpanelDbContext context, ISshHostKeyScanner scanner)
    {
        _context = context;
        _scanner = scanner;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The presented key, or <c>SERVER_NOT_FOUND</c>, <c>SSH_SETTINGS_INCOMPLETE</c> (no agent host
    /// saved), <c>SSH_HOST_KEY_UNREADABLE</c>.
    /// </returns>
    public async Task<Result<SshHostKeyScanDto>> HandleAsync(ScanServerSshHostKeyCommand cmd, CancellationToken ct)
    {
        if (!await _context.Servers.AnyAsync(s => s.Id == cmd.ServerId, ct))
        {
            return Result<SshHostKeyScanDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var settings = await _context.ServerSshSettings.AsNoTracking().FirstOrDefaultAsync(s => s.ServerId == cmd.ServerId, ct);
        if (settings is null || string.IsNullOrWhiteSpace(settings.AgentHost))
        {
            return Result<SshHostKeyScanDto>.FailureCode(HostpanelErrorCodes.SSH_SETTINGS_INCOMPLETE);
        }

        var scan = await _scanner.ScanAsync(
            settings.AgentHost, settings.AgentPort, TimeSpan.FromSeconds(settings.ConnectTimeoutSeconds), ct);
        if (scan is null)
        {
            return Result<SshHostKeyScanDto>.FailureCode(HostpanelErrorCodes.SSH_HOST_KEY_UNREADABLE);
        }

        return Result<SshHostKeyScanDto>.Success(new SshHostKeyScanDto(
            scan.Algorithm, scan.Fingerprint, string.Equals(scan.Fingerprint, settings.AgentHostKeyFingerprint, StringComparison.Ordinal)));
    }
}
