namespace Innovayse.Hostpanel.Domain.Ssh;

/// <summary>What a provider reports about an account's SSH access.</summary>
/// <param name="ShellEnabled">Whether the account has a login shell, i.e. SSH is enabled for it.</param>
/// <param name="ManagedKeyCount">Keys HostPanel installed and the server still holds.</param>
/// <param name="UnmanagedKeyCount">Keys the customer installed by other means; never touched.</param>
public sealed record SshAccessStatus(bool ShellEnabled, int ManagedKeyCount, int UnmanagedKeyCount);
