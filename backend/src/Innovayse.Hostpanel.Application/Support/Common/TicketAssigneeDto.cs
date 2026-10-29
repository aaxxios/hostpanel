namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>The staff member a ticket is assigned to.</summary>
/// <param name="Id">FK to the staff member assigned, if any.</param>
/// <param name="Name">Display name of the assigned staff member, if any.</param>
public sealed record TicketAssigneeDto(int? Id, string? Name);
