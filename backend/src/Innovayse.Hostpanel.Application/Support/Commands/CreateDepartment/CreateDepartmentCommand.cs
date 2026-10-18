namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDepartment;

/// <summary>Command to create a new support department.</summary>
/// <param name="Name">The department display name.</param>
/// <param name="Email">The department email address.</param>
public sealed record CreateDepartmentCommand(string Name, string Email);
