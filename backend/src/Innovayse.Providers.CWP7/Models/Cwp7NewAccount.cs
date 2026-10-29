namespace Innovayse.Providers.CWP7.Models;

/// <summary>The account a CWP7 server is asked to create.</summary>
/// <param name="Domain">Primary domain for the account.</param>
/// <param name="Username">Username to create.</param>
/// <param name="Password">Account password.</param>
/// <param name="Package">Hosting package name.</param>
/// <param name="Email">Contact email address.</param>
/// <param name="InodeLimit">Inode limit; 0 means unlimited.</param>
/// <param name="ProcessLimit">Most processes the account may run at once.</param>
/// <param name="OpenFileLimit">Most files the account may hold open at once.</param>
/// <param name="ServerIp">Server IP address the account is bound to.</param>
internal sealed record Cwp7NewAccount(
    string Domain,
    string Username,
    string Password,
    string Package,
    string Email,
    int InodeLimit,
    int ProcessLimit,
    int OpenFileLimit,
    string ServerIp);
