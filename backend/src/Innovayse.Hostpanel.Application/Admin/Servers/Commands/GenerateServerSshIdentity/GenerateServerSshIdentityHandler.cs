using Innovayse.Hostpanel.Application.Admin.Servers.Common;
using Innovayse.Hostpanel.Application.Admin.Servers.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.GenerateServerSshIdentity;

/// <summary>
/// Handles <see cref="GenerateServerSshIdentityCommand"/>: a new ED25519 pair for this server only.
/// The private half is stored encrypted and never leaves HostPanel; the public half goes into the
/// installer command. Rotating switches customer access off until the agent is reinstalled.
/// </summary>
public sealed class GenerateServerSshIdentityHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Provider factory, for support and the agent profile.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Key pair generator.</summary>
    private readonly ISshIdentityGenerator _generator;

    /// <summary>The bundle this build ships.</summary>
    private readonly ISshAgentBundle _bundle;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="factory">Provider factory, for support and the agent profile.</param>
    /// <param name="generator">Key pair generator.</param>
    /// <param name="bundle">The bundle this build ships.</param>
    public GenerateServerSshIdentityHandler(
        IHostpanelDbContext context, ISshKeyProviderFactory factory, ISshIdentityGenerator generator, ISshAgentBundle bundle)
    {
        _context = context;
        _factory = factory;
        _generator = generator;
        _bundle = bundle;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The setup with the new public key, or <c>SERVER_NOT_FOUND</c> / <c>SSH_MODULE_NOT_SUPPORTED</c>.</returns>
    public async Task<Result<ServerSshSetupDto>> HandleAsync(GenerateServerSshIdentityCommand cmd, CancellationToken ct)
    {
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.ServerId, ct);
        if (server is null)
        {
            return Result<ServerSshSetupDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var profile = _factory.AgentProfileFor(server.Module);
        if (profile is null)
        {
            return Result<ServerSshSetupDto>.FailureCode(HostpanelErrorCodes.SSH_MODULE_NOT_SUPPORTED);
        }

        var settings = await _context.ServerSshSettings.FirstOrDefaultAsync(s => s.ServerId == server.Id, ct);
        if (settings is null)
        {
            settings = ServerSshSettings.CreateFor(server.Id);
            _context.ServerSshSettings.Add(settings);
        }

        var identity = _generator.Generate($"hostpanel-server-{server.Id}");
        settings.IssueIdentity(identity.PrivateKey, identity.PublicKey);
        await _context.SaveChangesAsync(ct);
        return Result<ServerSshSetupDto>.Success(settings.ToSetupDto(server.Id, profile, _bundle));
    }
}
