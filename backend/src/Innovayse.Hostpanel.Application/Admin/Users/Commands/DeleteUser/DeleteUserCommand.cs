using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete an account. Client records that reference it are preserved as orphans.
/// </summary>
/// <param name="Id">The person's subject.</param>
public sealed record DeleteUserCommand(string Id) : IDemoRestricted;
