using Innovayse.Hostpanel.Domain.Services.Enums;

namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>DTO representing a cancellation request with enriched service and client data.</summary>
/// <param name="Id">Primary key of the cancellation request.</param>
/// <param name="ServiceId">FK to the client service being cancelled.</param>
/// <param name="ServiceName">Product name of the linked service; every request has one, since the request goes with its service and the service's product cannot be deleted.</param>
/// <param name="ClientId">FK to the client that owns the service.</param>
/// <param name="ClientName">Full name of that client; every request has one, since a request goes with its service and the service's client cannot be deleted.</param>
/// <param name="Type">
/// When the cancellation takes effect. Sent as the member name, not display text: the caller owns
/// the wording, in its own language.
/// </param>
/// <param name="Reason">Optional client-supplied reason.</param>
/// <param name="Status">Whether the request is still open. Sent as the member name, for the same reason as <paramref name="Type"/>.</param>
/// <param name="CreatedAt">UTC timestamp when the request was submitted.</param>
public sealed record CancellationRequestDto(
    int Id,
    int ServiceId,
    string ServiceName,
    int ClientId,
    string ClientName,
    CancellationType Type,
    string? Reason,
    CancellationStatus Status,
    DateTimeOffset CreatedAt);
