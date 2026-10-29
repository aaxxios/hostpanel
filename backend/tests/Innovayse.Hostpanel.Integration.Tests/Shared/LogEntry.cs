using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>One line a host logged.</summary>
/// <param name="Level">The level it was logged at.</param>
/// <param name="Message">The formatted message.</param>
internal sealed record LogEntry(LogLevel Level, string Message);
