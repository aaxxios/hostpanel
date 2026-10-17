using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Commands.SaveIntegrationConfig;

/// <summary>Validates <see cref="SaveIntegrationConfigCommand"/> before it reaches the handler.</summary>
public sealed class SaveIntegrationConfigValidator : LocalizedValidator<SaveIntegrationConfigCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SaveIntegrationConfigValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage(L["IntegrationSlugRequired"]);
        RuleFor(x => x.Config).NotNull().WithMessage(L["IntegrationConfigRequired"]);
    }
}
