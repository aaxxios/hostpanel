namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>Whether initial setup is outstanding. Claiming it always needs the token this installation printed to its log.</summary>
/// <param name="Required">True while nobody holds the Admin role.</param>
public sealed record SetupRequiredDto(bool Required);
