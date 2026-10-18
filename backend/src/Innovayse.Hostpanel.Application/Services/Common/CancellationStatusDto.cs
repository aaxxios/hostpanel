using Innovayse.Hostpanel.Domain.Services.Enums;
namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>DTO indicating whether a service has a pending cancellation request.</summary>
/// <param name="Pending"><see langword="true"/> if an open cancellation request exists.</param>
/// <param name="Type">
/// The <see cref="Innovayse.Hostpanel.Domain.Services.Enums.CancellationType"/> member name -- <c>Immediate</c>
/// or <c>EndOfBillingPeriod</c> -- if pending, or <see langword="null"/>. A machine-readable
/// value, never display text: the caller owns the wording, in its own language.
/// </param>
public record CancellationStatusDto(bool Pending, string? Type);
