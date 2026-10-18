using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Commands.StartMigrationPull;

/// <summary>
///     Validator for StartMigrationPullCommand. Whether the id names a pending job is the
///     handler's answer; this checks only that it is shaped like one.
/// </summary>
public sealed class StartMigrationPullValidator : LocalizedValidator<StartMigrationPullCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public StartMigrationPullValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["MigrationJobIdInvalid"]);
    }
}
