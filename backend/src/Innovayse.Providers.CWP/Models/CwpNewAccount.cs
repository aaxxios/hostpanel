namespace Innovayse.Providers.CWP.Models;

/// <summary>The account a CWP server is asked to create.</summary>
/// <param name="Domain">Primary domain for the account.</param>
/// <param name="Username">Username to create.</param>
/// <param name="Password">Account password.</param>
/// <param name="Package">Hosting package name.</param>
/// <param name="Email">Contact email address.</param>
public sealed record CwpNewAccount(
    string Domain,
    string Username,
    string Password,
    string Package,
    string Email);
