using Innovayse.Hostpanel.Domain.Services.Enums;

namespace Innovayse.Hostpanel.Application.Services.Commands.SuspendService;

/// <summary>Command to suspend an active client service.</summary>
/// <param name="ServiceId">The service primary key.</param>
/// <param name="Source">
/// Why. The admin route never sets it (it binds the id from the route only), so staff suspensions
/// are <see cref="SuspensionSource.Manual"/>; the billing engine and domain expiry pass their own,
/// which decides whether a later payment or renewal may lift the suspension.
/// </param>
public record SuspendServiceCommand(int ServiceId, SuspensionSource Source = SuspensionSource.Manual);
