using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationLogs;

/// <summary>
///     Validator for GetMigrationLogsQuery. Checks the job id and that each optional filter, when
///     present, names a member of its enum — a rule this handler used to resolve itself with a
///     hand-thrown <see cref="InvalidOperationException"/>, which is exactly the shape a validator
///     exists for. Page and page size carry safe defaults the handler does not clamp further, so
///     no rule is declared for them — decided, not forgotten.
/// </summary>
public sealed class GetMigrationLogsValidator : LocalizedValidator<GetMigrationLogsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetMigrationLogsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0)
            .WithMessage(L["MigrationJobIdInvalid"]);

        RuleFor(x => x.Action)
            .Must(value => value is null || Enum.TryParse<MigrationLogAction>(value, true, out _))
            .WithMessage(x => L["InvalidFilterValue", x.Action!]);

        RuleFor(x => x.EntityType)
            .Must(value => value is null || Enum.TryParse<MigrationEntityType>(value, true, out _))
            .WithMessage(x => L["InvalidFilterValue", x.EntityType!]);
    }
}
