namespace Innovayse.Hostpanel.Application.Provisioning.Common;

/// <summary>Input of a hosting account reactivation.</summary>
/// <param name="Account">The suspended account to reactivate.</param>
public sealed record HostedAccountUnsuspension(HostedAccountTarget Account);
