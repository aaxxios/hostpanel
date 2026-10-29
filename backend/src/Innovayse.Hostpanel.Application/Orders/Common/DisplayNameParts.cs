namespace Innovayse.Hostpanel.Application.Orders.Common;

/// <summary>A display name split into the first and last name a client row stores.</summary>
/// <param name="First">The first name, or <see langword="null"/> when the name does not supply it.</param>
/// <param name="Last">The last name, or <see langword="null"/> when the name does not supply it.</param>
internal sealed record DisplayNameParts(string? First, string? Last);
