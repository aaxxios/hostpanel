namespace Innovayse.Hostpanel.Application.Support.Seeding;

/// <summary>A department created on a fresh install.</summary>
/// <param name="Name">The department name.</param>
/// <param name="Email">The department contact address.</param>
internal sealed record DefaultDepartment(string Name, string Email);
