using Innovayse.Hostpanel.Domain.Services.Enums;

namespace Innovayse.Hostpanel.Application.Services.Commands.SuspendService;

/// <summary>
/// Suspends an active client service: on the panel its hosting account lives on first, then on the
/// platform. The one use case every suspension goes through, from staff, the billing runs and domain expiry.
/// </summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="Source">
/// Why. Staff routes always overlay <see cref="SuspensionSource.Manual"/>; the billing engine and domain
/// expiry pass their own, which decides whether a later payment or renewal may lift the suspension.
/// </param>
/// <param name="Reason">Human-readable reason, passed on to the panel; or <see langword="null"/> when the caller gives none.</param>
public sealed record SuspendServiceCommand(int ServiceId, SuspensionSource Source = SuspensionSource.Manual, string? Reason = null);
