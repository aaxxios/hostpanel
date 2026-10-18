using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceNotes;

/// <summary>Validates <see cref="UpdateInvoiceNotesCommand"/>.</summary>
public sealed class UpdateInvoiceNotesValidator : LocalizedValidator<UpdateInvoiceNotesCommand>
{
    /// <summary>Maximum length of the notes text.</summary>
    private const int NotesMaxLength = 2000;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateInvoiceNotesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
        RuleFor(x => x.Notes)
            .MaximumLength(NotesMaxLength)
            .When(x => x.Notes is not null)
            .WithMessage(L["InvoiceNotesTooLong", NotesMaxLength]);
    }
}
