using FluentValidation;
using Innovayse.Hostpanel.Application.Billing.Commands.RefundInvoicePayment.Enums;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.RefundInvoicePayment;

/// <summary>Validates <see cref="RefundInvoicePaymentCommand"/>.</summary>
public sealed class RefundInvoicePaymentValidator : LocalizedValidator<RefundInvoicePaymentCommand>
{
    /// <summary>Maximum length of the gateway name.</summary>
    private const int GatewayMaxLength = 100;

    /// <summary>Maximum length of the notes field.</summary>
    private const int NotesMaxLength = 1000;

    /// <summary>Valid refund type values.</summary>
    private static readonly string[] ValidRefundTypes = Enum.GetNames<RefundType>();

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RefundInvoicePaymentValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage(L["InvoiceRefundAmountInvalid"]);
        RuleFor(x => x.RefundType)
            .Must(t => ValidRefundTypes.Contains(t))
            .WithMessage(L["InvoiceRefundTypeInvalid"]);

        // Required only where the handler reads it. RefundInvoicePaymentHandler overwrites
        // Gateway with the literal "Credit Balance" when RefundType is CreditBalance, so demanding
        // a non-empty value for that branch would refuse a request over a field that is discarded.
        RuleFor(x => x.Gateway)
            .NotEmpty()
            .When(x => x.RefundType != nameof(RefundType.CreditBalance))
            .WithMessage(L["InvoiceRefundGatewayInvalid"]);

        RuleFor(x => x.Gateway)
            .MaximumLength(GatewayMaxLength)
            .When(x => x.Gateway is not null)
            .WithMessage(L["InvoiceRefundGatewayMaxLengthInvalid", GatewayMaxLength]);
        RuleFor(x => x.RefundTransactionId)
            .NotEmpty()
            .When(x => x.RefundType == nameof(RefundType.Manual))
            .WithMessage(L["InvoiceRefundTransactionIdRequiredInvalid"]);
        RuleFor(x => x.Notes)
            .MaximumLength(NotesMaxLength)
            .When(x => x.Notes is not null)
            .WithMessage(L["InvoiceRefundNotesInvalid", NotesMaxLength]);
    }
}
