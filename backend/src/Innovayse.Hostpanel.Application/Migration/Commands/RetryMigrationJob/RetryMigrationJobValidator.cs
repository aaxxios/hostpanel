using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Commands.RetryMigrationJob;

/// <summary>
///     Validator for RetryMigrationJobCommand. Whether the id names a failed job is the handler's
///     answer; this checks only that it is shaped like one.
/// </summary>
public sealed class RetryMigrationJobValidator : LocalizedValidator<RetryMigrationJobCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RetryMigrationJobValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["MigrationJobIdInvalid"]);
    }
}
