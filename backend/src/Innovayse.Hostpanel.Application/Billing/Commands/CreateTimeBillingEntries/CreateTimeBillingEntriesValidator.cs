using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateTimeBillingEntries;

/// <summary>Validates <see cref="CreateTimeBillingEntriesCommand"/> before it reaches the handler.</summary>
public sealed class CreateTimeBillingEntriesValidator : LocalizedValidator<CreateTimeBillingEntriesCommand>
{
    /// <summary>Maximum length of a time entry description.</summary>
    private const int DescriptionMaxLength = 500;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateTimeBillingEntriesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["BillableItemClientIdInvalid"]);
        RuleFor(x => x.Entries).NotEmpty().WithMessage(L["TimeBillingEntriesRequired"]);
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.Description)
                .NotEmpty()
                .MaximumLength(DescriptionMaxLength)
                .WithMessage(L["TimeBillingDescriptionInvalid", DescriptionMaxLength]);
            entry.RuleFor(e => e.Hours).GreaterThan(0).WithMessage(L["TimeBillingHoursInvalid"]);
            entry.RuleFor(e => e.Rate).GreaterThanOrEqualTo(0).WithMessage(L["TimeBillingRateInvalid"]);
        });
    }
}
