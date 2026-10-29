namespace Innovayse.Hostpanel.Application.Services.Commands.TerminateService;

/// <summary>
/// Permanently terminates a client service: its hosting account is removed from the panel first, then
/// the service is marked terminated on the platform.
/// </summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="Reason">Human-readable reason, passed on to the panel; or <see langword="null"/> when the caller gives none.</param>
public sealed record TerminateServiceCommand(int ServiceId, string? Reason = null);
