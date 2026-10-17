using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.ModifyMyDomainContact;

/// <summary>Validates <see cref="ModifyMyDomainContactCommand"/> before it reaches the handler.</summary>
public sealed class ModifyMyDomainContactValidator : LocalizedValidator<ModifyMyDomainContactCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ModifyMyDomainContactValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).MustBePositiveId(L["DomainIdRequired"]);
        RuleFor(x => x.Contact).NotNull().WithMessage(L["DomainContactRequired"]);
        RuleFor(x => x.Contact.FirstName).NotEmpty().WithMessage(L["DomainContactFirstNameRequired"]).When(x => x.Contact is not null);
        RuleFor(x => x.Contact.LastName).NotEmpty().WithMessage(L["DomainContactLastNameRequired"]).When(x => x.Contact is not null);
        RuleFor(x => x.Contact.Email)
            .NotEmpty().WithMessage(L["DomainContactEmailRequired"])
            .EmailAddress().WithMessage(L["DomainContactEmailInvalid"])
            .When(x => x.Contact is not null);
        RuleFor(x => x.Contact.Phone).NotEmpty().WithMessage(L["DomainContactPhoneRequired"]).When(x => x.Contact is not null);
        RuleFor(x => x.Contact.Address1).NotEmpty().WithMessage(L["DomainContactAddress1Required"]).When(x => x.Contact is not null);
        RuleFor(x => x.Contact.City).NotEmpty().WithMessage(L["DomainContactCityRequired"]).When(x => x.Contact is not null);
        RuleFor(x => x.Contact.State).NotEmpty().WithMessage(L["DomainContactStateRequired"]).When(x => x.Contact is not null);
        RuleFor(x => x.Contact.PostalCode).NotEmpty().WithMessage(L["DomainContactPostalCodeRequired"]).When(x => x.Contact is not null);
        RuleFor(x => x.Contact.Country).NotEmpty().WithMessage(L["DomainContactCountryRequired"]).When(x => x.Contact is not null);
    }
}
