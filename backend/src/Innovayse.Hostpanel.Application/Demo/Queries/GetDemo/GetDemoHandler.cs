using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Demo.Common;
using Innovayse.Hostpanel.Application.Demo.Common.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Application.Demo.Queries.GetDemo;

/// <summary>
/// Handles <see cref="GetDemoQuery"/>: publishes the demo accounts and the next reset time, or the
/// fixed "not a demo" answer, which carries no account and no time.
/// </summary>
public sealed class GetDemoHandler
{
    /// <summary>The demo switch and the published accounts.</summary>
    private readonly IOptions<DemoOptions> _options;

    /// <summary>When the next reset runs.</summary>
    private readonly IDemoResetSchedule _schedule;

    /// <summary>Initializes the handler.</summary>
    /// <param name="options">The demo switch and the published accounts.</param>
    /// <param name="schedule">When the next reset runs.</param>
    public GetDemoHandler(IOptions<DemoOptions> options, IDemoResetSchedule schedule)
    {
        _options = options;
        _schedule = schedule;
    }

    /// <summary>Builds the answer.</summary>
    /// <param name="query">The query (no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the demo status; this use case refuses nothing.</returns>
    public Task<Result<DemoDto>> HandleAsync(GetDemoQuery query, CancellationToken ct)
    {
        _ = query;
        _ = ct;

        var options = _options.Value;
        if (!options.Enabled)
        {
            return Task.FromResult(Result.Success(new DemoDto(false, null, [])));
        }

        IReadOnlyList<DemoAccountDto> accounts =
        [
            new DemoAccountDto(DemoAccountRole.Admin, options.AdminEmail!, options.AdminPassword!),
            new DemoAccountDto(DemoAccountRole.Client, options.ClientEmail!, options.ClientPassword!),
        ];

        return Task.FromResult(Result.Success(new DemoDto(true, _schedule.NextResetAt, accounts)));
    }
}
