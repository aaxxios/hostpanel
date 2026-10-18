using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpdateProductPrices;

/// <summary>
///     Validator for UpdateProductPricesCommand.
///     Checks the shape of each code; whether one is configured, or is the base, is the handler's answer.
/// </summary>
public sealed class UpdateProductPricesValidator : LocalizedValidator<UpdateProductPricesCommand>
{
    /// <summary>Length of an ISO 4217 alpha code.</summary>
    private const int CodeLength = 3;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateProductPricesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Currencies)
            .NotEmpty()
            .WithMessage(L["CurrenciesRequired"]);

        RuleForEach(x => x.Currencies)
            .NotEmpty()
            .Length(CodeLength)
            .Must(code => code.All(char.IsAsciiLetter))
            .WithMessage(L["CurrencyCodeInvalid"]);
    }
}
