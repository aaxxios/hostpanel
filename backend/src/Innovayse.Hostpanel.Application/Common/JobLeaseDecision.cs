namespace Innovayse.Hostpanel.Application.Common;

/// <summary>The answer to a <see cref="JobLeaseClaim"/>.</summary>
/// <param name="Acquired">Whether the caller now holds the lease.</param>
/// <param name="HeldBy">Who holds the lease when the caller does not; <see langword="null"/> when the caller got it.</param>
public sealed record JobLeaseDecision(bool Acquired, string? HeldBy);
