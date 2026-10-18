using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateQuote;

/// <summary>Validates <see cref="CreateQuoteCommand"/> before it reaches the handler.</summary>
public sealed class CreateQuoteValidator : LocalizedValidator<CreateQuoteCommand>
{
    /// <summary>Maximum length of the quote subject.</summary>
    private const int SubjectMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateQuoteValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["QuoteClientIdInvalid"]);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(SubjectMaxLength).WithMessage(L["QuoteSubjectInvalid"]);
        RuleFor(x => x.Items).NotEmpty().WithMessage(L["QuoteItemsRequired"]);
        RuleForEach(x => x.Items).SetValidator(new QuoteItemEntryValidator(localizer));
    }
}
