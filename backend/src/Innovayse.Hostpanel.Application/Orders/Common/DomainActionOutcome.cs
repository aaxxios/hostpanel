namespace Innovayse.Hostpanel.Application.Orders.Common;

/// <summary>What fulfilling the domain action of one order line produced.</summary>
/// <param name="DomainId">
/// The new domain's id when the action created one (register, transfer) and it succeeded; otherwise
/// <see langword="null"/>.
/// </param>
/// <param name="ErrorCode">The code the refusing use case answered with, or <see langword="null"/> on success or when it gave none.</param>
/// <param name="FailureReason">
/// <see langword="null"/> on success, otherwise a human-readable sentence for the refund and the log.
/// </param>
internal sealed record DomainActionOutcome(int? DomainId, string? ErrorCode, string? FailureReason);
