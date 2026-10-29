using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.SendPasswordReset;

/// <summary>
/// Command to send a user a link they can use to choose a new password (admin action).
/// </summary>
/// <param name="Id">The person's subject.</param>
public sealed record SendPasswordResetCommand(string Id) : IDemoRestricted;
