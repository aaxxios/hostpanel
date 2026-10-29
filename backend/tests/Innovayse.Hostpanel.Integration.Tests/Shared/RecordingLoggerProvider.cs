using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>A logger provider that keeps every log line, for asserting what a host logged.</summary>
internal sealed class RecordingLoggerProvider : ILoggerProvider
{
    /// <summary>Every line logged so far, across all categories.</summary>
    public ConcurrentQueue<LogEntry> Entries { get; } = new();

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new RecordingLogger(Entries);

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
