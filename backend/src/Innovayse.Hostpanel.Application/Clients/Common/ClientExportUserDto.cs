using Innovayse.Hostpanel.Domain.Clients.Enums;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>One person linked to the account besides its owner, as the <c>users</c> section of a client data export lists it.</summary>
/// <param name="UserId">The person's identity subject.</param>
/// <param name="Email">Sign-in address, read from the identity store; empty when the identity store no longer knows the person.</param>
/// <param name="Permissions">What the person may do on the account.</param>
/// <param name="CreatedAt">When the person was linked to the account.</param>
public sealed record ClientExportUserDto(
    string UserId,
    string Email,
    ClientPermission Permissions,
    DateTimeOffset CreatedAt);
