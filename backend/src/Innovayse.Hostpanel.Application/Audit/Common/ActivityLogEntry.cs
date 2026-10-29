namespace Innovayse.Hostpanel.Application.Audit.Common;

/// <summary>
/// One line for the activity log, as another module reports it through
/// <see cref="Innovayse.Hostpanel.Application.Common.Interfaces.IActivityLogger"/>. Who acted and
/// from where is taken from the current caller, never from the entry.
/// </summary>
/// <param name="ClientId">The client record the action concerns; <see langword="null"/> for a system-wide action such as a plugin install.</param>
/// <param name="Description">What happened, in the operator's words (for example "Cancelled Invoice - Invoice ID: 12").</param>
public sealed record ActivityLogEntry(int? ClientId, string Description);
