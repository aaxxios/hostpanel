using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Commands.DeleteMigrationJob;

/// <summary>
///     Validator for DeleteMigrationJobCommand. Whether the id names a job that still exists is
///     the handler's answer; this checks only that it is shaped like one.
/// </summary>
public sealed class DeleteMigrationJobValidator : LocalizedValidator<DeleteMigrationJobCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DeleteMigrationJobValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage(L["MigrationJobIdInvalid"]);
    }
}
