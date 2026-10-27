using Innovayse.Hostpanel.Domain.Ssh;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>Generates per-server ED25519 management identities.</summary>
public interface ISshIdentityGenerator
{
    /// <summary>Generates a new key pair.</summary>
    /// <param name="comment">Comment for the public key line, e.g. <c>hostpanel-server-12</c>.</param>
    /// <returns>The pair.</returns>
    public SshIdentity Generate(string comment);
}
