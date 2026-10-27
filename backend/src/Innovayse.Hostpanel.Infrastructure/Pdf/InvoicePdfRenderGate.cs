using Innovayse.Hostpanel.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Pdf;

/// <summary>
/// The process-wide limit on how many invoice PDFs render at once.
/// </summary>
/// <remarks>
/// <para>
/// Rendering is synchronous CPU work that holds a thread-pool thread from start to finish. The
/// per-caller rate-limit policy stops one client from starting many, but many clients at once, or
/// staff on the admin route, which has no policy, could still occupy every pool thread. One gate
/// for the whole process caps that at <see cref="InvoicePdfOptions.MaxConcurrentRenders"/>.
/// </para>
/// <para>
/// A render over the cap waits asynchronously, holding no thread, and leaves the queue if its
/// request is cancelled. Registered as a singleton, so every renderer shares the one semaphore;
/// the container disposes it at shutdown.
/// </para>
/// </remarks>
public sealed class InvoicePdfRenderGate : IDisposable
{
    /// <summary>One permit per render allowed in flight.</summary>
    private readonly SemaphoreSlim _slots;

    /// <summary>Initializes the gate.</summary>
    /// <param name="options">How many renders may run at once.</param>
    public InvoicePdfRenderGate(IOptions<InvoicePdfOptions> options)
    {
        var limit = options.Value.MaxConcurrentRenders;
        _slots = new SemaphoreSlim(limit, limit);
    }

    /// <summary>Waits for a free slot and takes it.</summary>
    /// <param name="ct">Cancels the wait; a cancelled wait never takes a slot.</param>
    /// <returns>The slot, given back when it is disposed.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is cancelled first.</exception>
    public async Task<IDisposable> EnterAsync(CancellationToken ct)
    {
        await _slots.WaitAsync(ct);
        return new Slot(_slots);
    }

    /// <inheritdoc/>
    public void Dispose() => _slots.Dispose();

    /// <summary>A taken slot; disposing it gives the permit back, once.</summary>
    /// <param name="slots">The semaphore the permit came from.</param>
    private sealed class Slot(SemaphoreSlim slots) : IDisposable
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
}
