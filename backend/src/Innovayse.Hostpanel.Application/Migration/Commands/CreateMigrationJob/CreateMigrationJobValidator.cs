using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Migration.Commands.CreateMigrationJob;

/// <summary>
///     Validator for CreateMigrationJobCommand. Checks only the shape of the source address; the
///     fifteen export flags are booleans, which every value of is valid.
/// </summary>
public sealed class CreateMigrationJobValidator : LocalizedValidator<CreateMigrationJobCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateMigrationJobValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Selection)
            .NotNull()
            .WithMessage(L["MigrationSelectionRequired"]);

        RuleFor(x => x.SourceUrl)
            .NotEmpty()
            .WithMessage(L["MigrationSourceUrlRequired"]);

        RuleFor(x => x.SourceUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.SourceUrl))
            .WithMessage(L["MigrationSourceUrlInvalid"]);
    }
}
