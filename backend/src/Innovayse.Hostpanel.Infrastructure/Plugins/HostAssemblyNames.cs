namespace Innovayse.Hostpanel.Infrastructure.Plugins;

/// <summary>
/// The assembly names a plugin package may not carry. A plugin folder is loaded with
/// <c>Assembly.LoadFrom</c> over every DLL in it, so a DLL named like one the host already uses
/// (the plugin SDK, a host assembly, or a framework or shipped dependency) would give the process a
/// second copy of those types and break every cast between host and plugin.
/// </summary>
internal static class HostAssemblyNames
{
    /// <summary>Name prefixes reserved for the host and the framework, whatever this host happens to ship.</summary>
    private static readonly string[] ReservedPrefixes = ["Innovayse.Hostpanel.", "System.", "Microsoft."];

    /// <summary>Exact names reserved for the plugin SDK and the framework's core assemblies.</summary>
    private static readonly string[] ReservedNames = ["Innovayse.SDK", "System", "mscorlib", "netstandard"];

    /// <summary>
    /// Every assembly the host resolves without help: the runtime's trusted platform assemblies,
    /// which cover the framework and the application's own dependencies.
    /// </summary>
    private static readonly Lazy<HashSet<string>> TrustedPlatformAssemblies = new(LoadTrustedPlatformAssemblies);

    /// <summary>Whether an archive entry is an assembly the host already provides.</summary>
    /// <param name="entryFileName">The entry's file name, without its folder.</param>
    /// <returns><see langword="true"/> for a <c>.dll</c> or <c>.exe</c> named like a host, SDK or framework assembly.</returns>
    public static bool IsHostAssembly(string entryFileName)
    {
        var extension = Path.GetExtension(entryFileName);
        if (!extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var name = Path.GetFileNameWithoutExtension(entryFileName);
        return ReservedNames.Contains(name, StringComparer.OrdinalIgnoreCase)
            || ReservedPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            || TrustedPlatformAssemblies.Value.Contains(name);
    }

    /// <summary>Reads the runtime's trusted platform assembly list into a set of simple names.</summary>
    /// <returns>The names, compared case-insensitively.</returns>
    private static HashSet<string> LoadTrustedPlatformAssemblies()
    {
        var list = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        return list
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
