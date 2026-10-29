namespace Innovayse.Hostpanel.Domain.Servers;

/// <summary>
/// Which stored server credentials an edit removes. The admin panel never receives a credential, so
/// an empty field means "unchanged"; an explicit clear is the only way to erase one.
/// </summary>
/// <param name="Password">Remove the stored API password.</param>
/// <param name="ApiToken">Remove the stored API token.</param>
/// <param name="AccessHash">Remove the stored access hash.</param>
public sealed record ServerCredentialClears(bool Password, bool ApiToken, bool AccessHash)
{
    /// <summary>Gets the value that removes nothing.</summary>
    public static ServerCredentialClears None { get; } = new(false, false, false);
}
