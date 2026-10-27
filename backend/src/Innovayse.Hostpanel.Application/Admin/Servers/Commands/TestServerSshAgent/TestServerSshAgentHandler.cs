using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.TestServerSshAgent;

/// <summary>
/// Handles <see cref="TestServerSshAgentCommand"/>. Works on disabled settings too, so a server
/// can be verified before it is switched on.
/// </summary>
public sealed class TestServerSshAgentHandler
{
    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Provider factory.</summary>
    private readonly ISshKeyProviderFactory _factory;

    /// <summary>Initialises the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="factory">Provider factory.</param>
    public TestServerSshAgentHandler(IHostpanelDbContext context, ISshKeyProviderFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The outcome (an unhealthy agent is a successful check), or <c>SERVER_NOT_FOUND</c>,
    /// <c>SSH_SETTINGS_INCOMPLETE</c>, <c>SSH_MODULE_NOT_SUPPORTED</c>.
    /// </returns>
    public async Task<Result<TestServerSshAgentResultDto>> HandleAsync(TestServerSshAgentCommand cmd, CancellationToken ct)
    {
        var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.ServerId, ct);
        if (server is null)
        {
            return Result<TestServerSshAgentResultDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var settings = await _context.ServerSshSettings.FirstOrDefaultAsync(s => s.ServerId == server.Id, ct);
        if (settings is null || !settings.IsComplete)
        {
            return Result<TestServerSshAgentResultDto>.FailureCode(HostpanelErrorCodes.SSH_SETTINGS_INCOMPLETE);
        }

        var provider = _factory.CreateFor(server, settings);
        if (provider is null)
        {
            return Result<TestServerSshAgentResultDto>.FailureCode(HostpanelErrorCodes.SSH_MODULE_NOT_SUPPORTED);
        }

        TestServerSshAgentResultDto outcome;
        try
        {
            outcome = new TestServerSshAgentResultDto(true, await provider.PingAsync(ct), null);
        }
        catch (SshProviderException ex)
        {
            outcome = new TestServerSshAgentResultDto(false, null, ex.Code);
        }

        settings.RecordHealthCheck(outcome.IsHealthy);
        await _context.SaveChangesAsync(ct);
        return Result<TestServerSshAgentResultDto>.Success(outcome);
    }
}
