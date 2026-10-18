using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.AddInvoicePayment;

/// <summary>Validates <see cref="AddInvoicePaymentCommand"/>.</summary>
public sealed class AddInvoicePaymentValidator : LocalizedValidator<AddInvoicePaymentCommand>
{
    /// <summary>Maximum length of the gateway name.</summary>
    private const int GatewayMaxLength = 100;

    /// <summary>Maximum length of the external transaction reference.</summary>
    private const int TransactionIdMaxLength = 255;

    /// <summary>Maximum length of the notes field.</summary>
    private const int NotesMaxLength = 1000;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AddInvoicePaymentValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
        RuleFor(x => x.Gateway)
            .NotEmpty()
            .MaximumLength(GatewayMaxLength)
            .WithMessage(L["InvoicePaymentGatewayInvalid", GatewayMaxLength]);
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .MaximumLength(TransactionIdMaxLength)
            .WithMessage(L["InvoicePaymentTransactionIdInvalid", TransactionIdMaxLength]);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage(L["InvoicePaymentAmountInvalid"]);
        RuleFor(x => x.Notes)
            .MaximumLength(NotesMaxLength)
            .When(x => x.Notes is not null)
            .WithMessage(L["InvoicePaymentNotesInvalid", NotesMaxLength]);
    }
}
