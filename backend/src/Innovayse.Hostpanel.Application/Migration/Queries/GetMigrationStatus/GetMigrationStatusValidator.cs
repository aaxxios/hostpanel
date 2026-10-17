using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationStatus;

/// <summary>
///     Validator for GetMigrationStatusQuery. Whether the id names a job that still exists is the
///     handler's answer; this checks only that it is shaped like one.
/// </summary>
public sealed class GetMigrationStatusValidator : LocalizedValidator<GetMigrationStatusQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetMigrationStatusValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage(L["MigrationJobIdInvalid"]);
    }
}
