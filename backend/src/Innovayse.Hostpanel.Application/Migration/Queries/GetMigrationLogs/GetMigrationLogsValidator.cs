using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationLogs;

/// <summary>
///     Validator for GetMigrationLogsQuery. Checks the job id and that each optional filter, when
///     present, is a defined member of its enum (a value the query string cannot name at all is
///     already refused by model binding). Page and page size carry safe defaults the handler does
///     not clamp further, so no rule is declared for them.
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
            .IsInEnum()
            .WithMessage(x => L["InvalidFilterValue", x.Action!.Value.ToString()]);

        RuleFor(x => x.EntityType)
            .IsInEnum()
            .WithMessage(x => L["InvalidFilterValue", x.EntityType!.Value.ToString()]);
    }
}
