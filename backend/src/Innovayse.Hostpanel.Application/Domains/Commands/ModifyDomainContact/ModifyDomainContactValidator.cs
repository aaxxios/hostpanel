using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.ModifyDomainContact;

/// <summary>Validates <see cref="ModifyDomainContactCommand"/> inputs before the handler executes.</summary>
public sealed class ModifyDomainContactValidator : LocalizedValidator<ModifyDomainContactCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ModifyDomainContactValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
        RuleFor(x => x.FirstName).NotEmpty().WithMessage(L["DomainContactFirstNameRequired"]);
        RuleFor(x => x.LastName).NotEmpty().WithMessage(L["DomainContactLastNameRequired"]);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(L["DomainContactEmailRequired"])
            .EmailAddress().WithMessage(L["DomainContactEmailInvalid"]);
        RuleFor(x => x.Phone).NotEmpty().WithMessage(L["DomainContactPhoneRequired"]);
        RuleFor(x => x.Address1).NotEmpty().WithMessage(L["DomainContactAddress1Required"]);
        RuleFor(x => x.City).NotEmpty().WithMessage(L["DomainContactCityRequired"]);
        RuleFor(x => x.State).NotEmpty().WithMessage(L["DomainContactStateRequired"]);
        RuleFor(x => x.PostalCode).NotEmpty().WithMessage(L["DomainContactPostalCodeRequired"]);
        RuleFor(x => x.Country).NotEmpty().WithMessage(L["DomainContactCountryRequired"]);
    }
}
