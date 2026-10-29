namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>The department a ticket is filed under.</summary>
/// <param name="Id">FK to the department this ticket is assigned to, if any.</param>
/// <param name="Name">Name of the assigned department, if any.</param>
public sealed record TicketDepartmentDto(int? Id, string? Name);
