using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Orders.Commands.ConfirmOrderPayment;

/// <summary>
///     Validator for ConfirmOrderPaymentCommand. Checks only the shape of the fields; whether the
///     order and payment token match, and whether the card gateway verifies the intent, are the handler's
///     answers.
/// </summary>
public sealed class ConfirmOrderPaymentValidator : LocalizedValidator<ConfirmOrderPaymentCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ConfirmOrderPaymentValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.OrderId).MustBePositiveId(L["OrderIdInvalid"]);
        RuleFor(x => x.PaymentIntentId).NotEmpty().WithMessage(L["PaymentIntentIdRequired"]);
    }
}
