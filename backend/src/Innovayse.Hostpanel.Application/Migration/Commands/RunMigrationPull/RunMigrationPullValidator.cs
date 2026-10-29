using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Commands.RunMigrationPull;

/// <summary>
///     Validator for RunMigrationPullCommand. Whether the id names a queued job is the worker's
///     answer; this checks only that it is shaped like one.
/// </summary>
public sealed class RunMigrationPullValidator : LocalizedValidator<RunMigrationPullCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RunMigrationPullValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["MigrationJobIdInvalid"]);
    }
}
