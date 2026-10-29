using Microsoft.Extensions.Hosting;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A host lifetime a test stops by hand, to show a job what a shutting-down host looks like.
/// </summary>
public sealed class StoppableLifetime : IHostApplicationLifetime
{
    /// <summary>Source of <see cref="ApplicationStopping"/>.</summary>
    private readonly CancellationTokenSource _stopping = new();

    /// <summary>Gets a token that is never cancelled; the cases here never read it.</summary>
    public CancellationToken ApplicationStarted => CancellationToken.None;

    /// <summary>Gets the token that is cancelled once <see cref="StopApplication"/> has been called.</summary>
    public CancellationToken ApplicationStopping => _stopping.Token;

    /// <summary>Gets a token that is never cancelled; the cases here never read it.</summary>
    public CancellationToken ApplicationStopped => CancellationToken.None;

    /// <summary>Marks the host as stopping.</summary>
    public void StopApplication()
    {
        _stopping.Cancel();
    }
}
