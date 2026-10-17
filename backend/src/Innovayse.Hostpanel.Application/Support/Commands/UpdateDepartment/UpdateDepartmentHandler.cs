using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateDepartment;

/// <summary>Updates an existing support department's name and email.</summary>
public sealed class UpdateDepartmentHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateDepartmentHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The department to update and its new name and email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>DEPARTMENT_NOT_FOUND</c> when the id names no department.</returns>
    public async Task<Result> HandleAsync(UpdateDepartmentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == cmd.Id, ct);
        if (department is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DEPARTMENT_NOT_FOUND);
        }

        department.Update(cmd.Name, cmd.Email);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
