using Innovayse.Hostpanel.Domain.Services.Enums;

namespace Innovayse.Hostpanel.Application.Services.Commands.ChangeServiceStatus;

/// <summary>Staff move a service to another status from the service page.</summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="Status">The status to move the service to.</param>
public sealed record ChangeServiceStatusCommand(int ServiceId, ServiceStatus Status);
