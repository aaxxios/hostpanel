namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// What it takes to create a locally stored account.
/// </summary>
/// <param name="Email">The account's email address, also used as its user name.</param>
/// <param name="Password">The plain-text password to hash and store.</param>
/// <param name="FirstName">Given name, or <see langword="null"/> when the person did not give one.</param>
/// <param name="LastName">Family name, or <see langword="null"/> when the person did not give one.</param>
public sealed record CreateUserInput(string Email, string Password, string? FirstName, string? LastName);
