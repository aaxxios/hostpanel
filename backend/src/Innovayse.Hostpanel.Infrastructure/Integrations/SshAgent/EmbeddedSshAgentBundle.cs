using System.Formats.Tar;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.SshAgent;

/// <summary>
/// <see cref="ISshAgentBundle"/> built once from the agent files embedded in this assembly
/// (<c>backend/agent/ssh-key-agent</c>, tests excluded). Entries are sorted and carry fixed times,
/// owners and modes, so the same build always produces the same bytes and the same SHA256.
/// </summary>
public sealed partial class EmbeddedSshAgentBundle : ISshAgentBundle
{
    /// <summary>Prefix of the agent files' manifest resource names.</summary>
    private const string ResourcePrefix = "SshAgent/";

    /// <summary>Fixed modification time of every entry.</summary>
    private static readonly DateTimeOffset EntryTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Initialises the bundle from the embedded files.</summary>
    public EmbeddedSshAgentBundle()
    {
        var assembly = typeof(EmbeddedSshAgentBundle).Assembly;
        var files = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name => (Path: name[ResourcePrefix.Length..], Bytes: Read(assembly, name)))
            .ToList();

        var agent = files.Single(f => f.Path == "hostpanel-ssh-key-agent");
        Version = VersionPattern().Match(System.Text.Encoding.UTF8.GetString(agent.Bytes)).Groups[1].Value;
        Profiles = files.Where(f => f.Path.StartsWith("profiles/", StringComparison.Ordinal) && f.Path.EndsWith(".conf", StringComparison.Ordinal))
            .Select(f => Path.GetFileNameWithoutExtension(f.Path)).ToList();

        var root = $"hostpanel-ssh-key-agent-{Version}/";
        using var buffer = new MemoryStream();
        using (var gzip = new GZipStream(buffer, CompressionLevel.Optimal, leaveOpen: true))
        using (var tar = new TarWriter(gzip, TarEntryFormat.Ustar, leaveOpen: true))
        {
            foreach (var (path, bytes) in files)
            {
                var executable = path is "hostpanel-ssh-key-agent" or "install.sh" or "uninstall.sh" || path.StartsWith("providers/", StringComparison.Ordinal);
                var entry = new UstarTarEntry(TarEntryType.RegularFile, root + path)
                {
                    ModificationTime = EntryTime,
                    Mode = executable
                        ? UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute
                        : UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead,
                    Uid = 0,
                    Gid = 0,
                    UserName = "root",
                    GroupName = "root",
                    DataStream = new MemoryStream(bytes),
                };
                tar.WriteEntry(entry);
            }
        }

        Content = buffer.ToArray();
        Sha256 = Convert.ToHexStringLower(SHA256.HashData(Content.Span));
    }

    /// <inheritdoc/>
    public string Version { get; }

    /// <inheritdoc/>
    public string Sha256 { get; }

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Content { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Profiles { get; }

    /// <summary>Reads one embedded file.</summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="name">Resource name.</param>
    /// <returns>Its bytes.</returns>
    private static byte[] Read(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    /// <summary>The agent's version line.</summary>
    /// <returns>The pattern.</returns>
    [GeneratedRegex("^AGENT_VERSION = \"([^\"]+)\"$", RegexOptions.Multiline)]
    private static partial Regex VersionPattern();
}
