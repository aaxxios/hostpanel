using Innovayse.Hostpanel.Application.Demo.Queries.GetDemo.Enums;

namespace Innovayse.Hostpanel.Application.Demo.Queries.GetDemo;

/// <summary>One published demo sign-in. The password is public by design: it is the demo's whole point.</summary>
/// <param name="Role">Which application the account signs in to.</param>
/// <param name="Email">The sign-in email.</param>
/// <param name="Password">The sign-in password.</param>
public sealed record DemoAccountDto(DemoAccountRole Role, string Email, string Password);
