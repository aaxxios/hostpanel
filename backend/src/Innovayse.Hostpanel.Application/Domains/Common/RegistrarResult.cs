namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Result returned by a registrar operation.
/// Wraps success/failure state and relevant registrar data.
/// </summary>
/// <param name="Success">Whether the registrar operation succeeded.</param>
/// <param name="RegistrarRef">Registrar-assigned reference/order ID; null on failure.</param>
/// <param name="ExpiresAt">Domain expiry date returned by the registrar; null when not applicable.</param>
/// <param name="ErrorCode">
/// Why the registrar refused, as a <see cref="Innovayse.Hostpanel.Application.Common.Errors.HostpanelErrorCodes"/> <c>REGISTRAR_*</c> code;
/// null on success. The provider's own wording is logged by the adapter, never carried here.
/// </param>
/// <param name="RequiresPolling">
/// When <see langword="true"/>, the registrar accepted the order but the domain will not be active immediately.
/// The caller must poll the registrar periodically until the domain becomes active.
/// </param>
public record RegistrarResult(
    bool Success,
    string? RegistrarRef,
    DateTimeOffset? ExpiresAt,
    string? ErrorCode,
    bool RequiresPolling = false);
