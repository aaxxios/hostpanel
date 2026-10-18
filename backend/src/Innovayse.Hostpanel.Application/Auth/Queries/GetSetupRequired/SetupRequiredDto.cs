namespace Innovayse.Hostpanel.Application.Auth.Queries.GetSetupRequired;

/// <summary>Whether initial setup is outstanding, and whether claiming it needs a token.</summary>
/// <param name="Required">True while nobody holds the Admin role.</param>
/// <param name="TokenRequired">True when claiming setup is gated on the token this installation printed to its log.</param>
public sealed record SetupRequiredDto(bool Required, bool TokenRequired);
