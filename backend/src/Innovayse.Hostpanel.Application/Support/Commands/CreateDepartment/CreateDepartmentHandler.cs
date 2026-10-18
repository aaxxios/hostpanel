using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDepartment;

/// <summary>Creates a new support department.</summary>
public sealed class CreateDepartmentHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateDepartmentHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The department's name and email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the new department's id. This use case answers no refusal code.</returns>
    public async Task<Result<int>> HandleAsync(CreateDepartmentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var department = Department.Create(cmd.Name, cmd.Email);

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(ct);

        return Result.Success(department.Id);
    }
}
