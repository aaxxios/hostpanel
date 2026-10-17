using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.ImportTldPricing;

/// <summary>
/// Validator for <see cref="ImportTldPricingCommand"/>. Whether the module names a known
/// registrar provider is the handler's answer.
/// </summary>
public sealed class ImportTldPricingValidator : LocalizedValidator<ImportTldPricingCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ImportTldPricingValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Module).NotEmpty().WithMessage(L["TldRegistrarModuleRequired"]);
    }
}
