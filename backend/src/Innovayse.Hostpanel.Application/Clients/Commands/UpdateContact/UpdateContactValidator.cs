using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateContact;

/// <summary>Validates <see cref="UpdateContactCommand"/> before the handler executes.</summary>
public sealed class UpdateContactValidator : LocalizedValidator<UpdateContactCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateContactValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.ContactId).MustBePositiveId(L["ClientContactIdInvalid"]);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage(L["ClientContactFirstNameInvalid"]);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage(L["ClientContactLastNameInvalid"]);
        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage(L["ClientContactCompanyNameInvalid"])
            .When(x => x.CompanyName is not null);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256).WithMessage(L["ClientContactEmailInvalid"]);
        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage(L["ClientContactPhoneInvalid"])
            .When(x => x.Phone is not null);
        RuleFor(x => x.Type).IsInEnum().WithMessage(L["ClientContactTypeInvalid"]);
        RuleFor(x => x.Street)
            .MaximumLength(200).WithMessage(L["ClientContactStreetInvalid"])
            .When(x => x.Street is not null);
        RuleFor(x => x.Address2)
            .MaximumLength(200).WithMessage(L["ClientContactAddress2Invalid"])
            .When(x => x.Address2 is not null);
        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage(L["ClientContactCityInvalid"])
            .When(x => x.City is not null);
        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage(L["ClientContactStateInvalid"])
            .When(x => x.State is not null);
        RuleFor(x => x.PostCode)
            .MaximumLength(20).WithMessage(L["ClientContactPostCodeInvalid"])
            .When(x => x.PostCode is not null);
        // Guarded on blank, not on null -- see AddContactValidator for why.
        RuleFor(x => x.Country)
            .Length(2).WithMessage(L["ClientContactCountryInvalid"])
            .When(x => !string.IsNullOrEmpty(x.Country));
    }
}
