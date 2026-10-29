namespace Innovayse.Hostpanel.Application.Provisioning.Common;

/// <summary>Input of a hosting account termination.</summary>
/// <param name="Account">The account to remove.</param>
/// <param name="Reason">Why it is terminated, passed on to the panel.</param>
public sealed record HostedAccountTermination(HostedAccountTarget Account, string Reason);
