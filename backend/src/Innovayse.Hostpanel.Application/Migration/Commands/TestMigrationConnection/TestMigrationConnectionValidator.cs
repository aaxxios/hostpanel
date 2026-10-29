using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Commands.TestMigrationConnection;

/// <summary>
///     Validator for TestMigrationConnectionCommand. Whether the id names a job that still exists is
///     the handler's answer; this checks only that it is shaped like one.
/// </summary>
public sealed class TestMigrationConnectionValidator : LocalizedValidator<TestMigrationConnectionCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public TestMigrationConnectionValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(L["MigrationJobIdInvalid"]);
    }
}
