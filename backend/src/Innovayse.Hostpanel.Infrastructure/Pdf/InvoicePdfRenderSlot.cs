namespace Innovayse.Hostpanel.Infrastructure.Pdf;

/// <summary>A slot taken from <see cref="InvoicePdfRenderGate"/>; disposing it gives the permit back, once.</summary>
/// <param name="slots">The semaphore the permit came from.</param>
internal sealed class InvoicePdfRenderSlot(SemaphoreSlim slots) : IDisposable
{
    /// <summary>Set once the permit has been given back, so a second dispose cannot over-release.</summary>
    private int _released;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _released, 1) != 0)
        {
            return;
        }

        try
        {
            slots.Release();
        }
        catch (ObjectDisposedException)
        {
            // The gate was disposed at shutdown while this render was still finishing. There is
            // nobody left to hand the permit to, and throwing here would replace the render's
            // own result or exception.
        }
    }
}
