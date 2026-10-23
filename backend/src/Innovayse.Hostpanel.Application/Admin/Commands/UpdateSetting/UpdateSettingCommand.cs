using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Commands.UpdateSetting;

/// <summary>Command that updates the value of an existing configuration setting.</summary>
/// <param name="Id">
/// The setting's primary key; wins over any id in the body. A value type, so it is left
/// non-nullable — a missing <c>int</c> in the body binds to <c>0</c> rather than being treated as
/// a required member, and the route overlay (<c>command with { Id = id }</c>) replaces it.
/// </param>
/// <param name="Value">
/// The new value to assign. For a secret setting an empty or absent value keeps what is stored,
/// because the admin panel never has the stored value to send back.
/// </param>
/// <param name="Clear">
/// Removes a secret setting's stored value. The explicit flag is what tells "clear it" apart from
/// "leave it as it is", which an empty value means for a secret.
/// </param>
public sealed record UpdateSettingCommand(int Id, string? Value, bool Clear = false) : IDemoRestricted;
