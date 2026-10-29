namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A staff member who appears in the demo activity log.</summary>
/// <param name="Id">The staff user id.</param>
/// <param name="Name">The name shown.</param>
/// <param name="Email">The email address.</param>
internal sealed record DemoStaffMember(string Id, string Name, string Email);
