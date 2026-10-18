using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateTransaction;

/// <summary>Validates <see cref="UpdateTransactionCommand"/> before it reaches the handler.</summary>
public sealed class UpdateTransactionValidator : LocalizedValidator<UpdateTransactionCommand>
{
    /// <summary>Maximum length of a transaction description.</summary>
    private const int DescriptionMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateTransactionValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["TransactionIdInvalid"]);
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(DescriptionMaxLength)
            .WithMessage(L["TransactionDescriptionInvalid", DescriptionMaxLength]);
        RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage(L["TransactionPaymentMethodInvalid"]);
        RuleFor(x => x.AmountIn).GreaterThanOrEqualTo(0).WithMessage(L["TransactionAmountInInvalid"]);
        RuleFor(x => x.AmountOut).GreaterThanOrEqualTo(0).WithMessage(L["TransactionAmountOutInvalid"]);
        RuleFor(x => x.Fees).GreaterThanOrEqualTo(0).WithMessage(L["TransactionFeesInvalid"]);
        RuleFor(x => x)
            .Must(x => x.AmountIn > 0 || x.AmountOut > 0)
            .WithMessage(L["TransactionAmountRequired"]);
    }
}
