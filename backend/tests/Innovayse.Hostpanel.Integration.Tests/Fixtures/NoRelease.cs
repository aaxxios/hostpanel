namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>A handle with nothing to release, since the gate never stops being suppressed.</summary>
internal sealed class NoRelease : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
