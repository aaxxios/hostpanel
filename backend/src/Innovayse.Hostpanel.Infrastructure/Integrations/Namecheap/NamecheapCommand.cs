namespace Innovayse.Hostpanel.Infrastructure.Integrations.Namecheap;

/// <summary>
/// One Namecheap API command with its own parameters. The credentials are not part of it:
/// <see cref="NamecheapClient"/> adds them to the request it sends and leaves this record alone.
/// </summary>
/// <param name="Name">The API command name (e.g. <c>namecheap.domains.create</c>).</param>
/// <param name="Parameters">The command-specific parameters.</param>
public sealed record NamecheapCommand(string Name, IReadOnlyDictionary<string, string> Parameters);
