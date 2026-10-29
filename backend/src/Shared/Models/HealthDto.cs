using Innovayse.Hostpanel.Shared.Models.Enums;

namespace Innovayse.Hostpanel.Shared.Models;

/// <summary>The liveness probe's answer.</summary>
/// <param name="Status">What the probe reports about the process.</param>
/// <param name="Timestamp">When the process answered, UTC.</param>
public sealed record HealthDto(HealthStatus Status, DateTimeOffset Timestamp);
