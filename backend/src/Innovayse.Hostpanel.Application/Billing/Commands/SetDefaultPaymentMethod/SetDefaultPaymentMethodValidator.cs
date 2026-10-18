using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SetDefaultPaymentMethod;

/// <summary>
///     Validator for <see cref="SetDefaultPaymentMethodCommand"/>. Checks the shape of the id and
///     that the caller asked for the one update this endpoint supports; ownership of the id is
///     the handler's answer.
/// </summary>
public sealed class SetDefaultPaymentMethodValidator : LocalizedValidator<SetDefaultPaymentMethodCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SetDefaultPaymentMethodValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.PaymentMethodId).NotEmpty();

        // The card gateway allows no other update to an existing card, so a request that does not ask to
        // make it the default is refused here rather than silently accepted and ignored.
        RuleFor(x => x.SetAsDefault)
            .Equal(true)
            .WithMessage(L["SetAsDefaultMustBeTrue"]);
    }
}
