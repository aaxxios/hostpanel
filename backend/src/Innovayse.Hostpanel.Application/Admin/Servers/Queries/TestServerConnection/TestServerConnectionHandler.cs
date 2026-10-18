using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.TestServerConnection;

/// <summary>Handles <see cref="TestServerConnectionQuery"/> -- tests connectivity and persists the result.</summary>
public sealed class TestServerConnectionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Probes a server's control-panel API to confirm the stored credentials still work.</summary>
    private readonly IServerConnectionTester _tester;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="tester">Probes a server's control-panel API to confirm the stored credentials still work.</param>
    public TestServerConnectionHandler(IHostpanelDbContext context, IServerConnectionTester tester)
    {
        _context = context;
        _tester = tester;
    }

    /// <summary>
    /// Runs the connection test, updates the server's status, and returns the result.
    /// </summary>
    /// <param name="query">The query containing the server id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The test result, whether or not the connection itself succeeded, or
    /// <c>SERVER_NOT_FOUND</c> when the id names no server. A failed connection is not a refusal
    /// of this request -- the diagnostic result is the answer the caller asked for.
    /// </returns>
    public async Task<Result<TestServerConnectionResultDto>> HandleAsync(
        TestServerConnectionQuery query, CancellationToken ct)
    {
        var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == query.ServerId, ct);
        if (server is null)
        {
            return Result<TestServerConnectionResultDto>.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var (connected, accountsCount, version, errorMessage) = await _tester.TestAsync(server, ct);

        server.RecordConnectionTest(connected, accountsCount);
        await _context.SaveChangesAsync(ct);

        return Result<TestServerConnectionResultDto>.Success(new TestServerConnectionResultDto(
            connected, accountsCount, version, errorMessage, server.LastTestedAt!.Value));
    }
}
