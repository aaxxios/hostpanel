using Innovayse.Hostpanel.Domain.Services.Enums;

namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>DTO indicating whether a service has a pending cancellation request.</summary>
/// <param name="Pending"><see langword="true"/> if an open cancellation request exists.</param>
/// <param name="Type">
/// When the open request takes effect, or <see langword="null"/> when none is pending. Sent as the
/// member name, never display text: the caller owns the wording, in its own language.
/// </param>
public sealed record CancellationStatusDto(bool Pending, CancellationType? Type);
