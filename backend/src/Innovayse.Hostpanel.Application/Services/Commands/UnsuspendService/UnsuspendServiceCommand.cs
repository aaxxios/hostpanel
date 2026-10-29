namespace Innovayse.Hostpanel.Application.Services.Commands.UnsuspendService;

/// <summary>
/// Re-activates a suspended client service: on the panel its hosting account lives on first, then on
/// the platform.
/// </summary>
/// <param name="ServiceId">The service primary key.</param>
public sealed record UnsuspendServiceCommand(int ServiceId);
