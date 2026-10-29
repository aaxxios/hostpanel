using Innovayse.Hostpanel.Domain.Auth.Enums;

namespace Innovayse.Hostpanel.Domain.Auth;

/// <summary>
/// Role names as compile-time constants, for the places that need a string constant such as
/// authorization attributes and role claims. Each value is the name of a <see cref="Role"/>
/// member, so the enum stays the one source of the vocabulary.
/// </summary>
public static class Roles
{
    /// <summary>Name of <see cref="Role.Admin"/>.</summary>
    public const string Admin = nameof(Role.Admin);

    /// <summary>Name of <see cref="Role.Reseller"/>.</summary>
    public const string Reseller = nameof(Role.Reseller);

    /// <summary>Name of <see cref="Role.Client"/>.</summary>
    public const string Client = nameof(Role.Client);
}
