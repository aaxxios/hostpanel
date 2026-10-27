namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// The SSH key agent installer bundle this HostPanel build ships: agent, installer, uninstaller,
/// provider profiles and helpers, as one deterministic <c>.tar.gz</c>. Its SHA256 is shown to the
/// administrator so the one-command installer verifies what it downloaded.
/// </summary>
public interface ISshAgentBundle
{
    /// <summary>Gets the agent version inside the bundle.</summary>
    public string Version { get; }

    /// <summary>Gets the lower-case hex SHA256 of <see cref="Content"/>.</summary>
    public string Sha256 { get; }

    /// <summary>Gets the bundle bytes.</summary>
    public ReadOnlyMemory<byte> Content { get; }

    /// <summary>Gets the provider profiles the bundle can install (e.g. <c>cwp</c>, <c>generic-linux</c>).</summary>
    public IReadOnlyList<string> Profiles { get; }
}
