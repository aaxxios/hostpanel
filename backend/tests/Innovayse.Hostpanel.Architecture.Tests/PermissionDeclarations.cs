using Innovayse.Hostpanel.API.Authorization;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// How the client portal actions of one controller declare their permissions.
/// </summary>
/// <param name="Required">The permission attributes the actions require.</param>
/// <param name="OpenToEveryUser">How many actions are open to every portal user.</param>
public sealed record PermissionDeclarations(IReadOnlyList<RequiresClientPermissionAttribute> Required, int OpenToEveryUser);
