namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>
/// An SSH endpoint to reach: where it listens and how long to wait for it to answer. Built by
/// HostPanel from the server's saved SSH settings, never from anything a browser sent.
/// </summary>
/// <param name="Host">The host name or address.</param>
/// <param name="Port">The SSH port.</param>
/// <param name="ConnectTimeout">How long to wait for the connection before giving up.</param>
public sealed record SshEndpoint(string Host, int Port, TimeSpan ConnectTimeout);
