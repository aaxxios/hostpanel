namespace Innovayse.Hostpanel.Shared.Models;

/// <summary>The HTTP status and title an error code is answered with.</summary>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Title">The response title.</param>
public sealed record ErrorStatus(int Status, string Title);
