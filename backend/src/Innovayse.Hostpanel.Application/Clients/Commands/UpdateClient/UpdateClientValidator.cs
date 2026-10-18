using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateClient;

/// <summary>Validates <see cref="UpdateClientCommand"/> before the handler executes.</summary>
public sealed class UpdateClientValidator : LocalizedValidator<UpdateClientCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateClientValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage(L["ClientFirstNameInvalid"]);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage(L["ClientLastNameInvalid"]);
        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage(L["ClientCompanyNameInvalid"])
            .When(x => x.CompanyName is not null);
        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage(L["ClientPhoneInvalid"])
            .When(x => x.Phone is not null);
        RuleFor(x => x.Street)
            .MaximumLength(200).WithMessage(L["ClientStreetInvalid"])
            .When(x => x.Street is not null);
        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage(L["ClientCityInvalid"])
            .When(x => x.City is not null);
        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage(L["ClientStateInvalid"])
            .When(x => x.State is not null);
        RuleFor(x => x.PostCode)
            .MaximumLength(20).WithMessage(L["ClientPostCodeInvalid"])
            .When(x => x.PostCode is not null);
        // Guarded on blank, not on null. An optional field a form left empty arrives as "" from
        // some callers and as null from others -- the admin panel itself sends both, depending on
        // the screen -- and `is not null` would put the empty string through a Length() rule it
        // cannot satisfy. Blank means "not provided".
        RuleFor(x => x.Country)
            .Length(2).WithMessage(L["ClientCountryInvalid"])
            .When(x => !string.IsNullOrEmpty(x.Country));
        // Checked against the one list that decides what this product answers in, rather than
        // against a literal here: an unlisted code would be stored, read back and then quietly
        // served English, which looks exactly like the save having failed.
        RuleFor(x => x.Language)
            .Must(l => LocaleOptions.SupportedLocales.Contains(l!, StringComparer.OrdinalIgnoreCase))
            .WithMessage(L["ClientLanguageInvalid"])
            .When(x => !string.IsNullOrEmpty(x.Language));
        // Exactly three, not merely "no more than three": Client.SetCurrency itself requires
        // exactly three characters, and a shorter (or empty) code used to slip past this rule and
        // throw there instead of answering a validation error here.
        RuleFor(x => x.Currency)
            .Length(3).WithMessage(L["ClientCurrencyInvalid"])
            .When(x => x.Currency is not null);
        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage(L["ClientPaymentMethodInvalid"])
            .When(x => x.PaymentMethod is not null);
        RuleFor(x => x.BillingContact)
            .MaximumLength(256).WithMessage(L["ClientBillingContactInvalid"])
            .When(x => x.BillingContact is not null);
        RuleFor(x => x.AdminNotes)
            .MaximumLength(2000).WithMessage(L["ClientAdminNotesInvalid"])
            .When(x => x.AdminNotes is not null);
        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<ClientStatus>(s, ignoreCase: true, out _))
            .WithMessage(L["ClientStatusInvalid"])
            .When(x => x.Status is not null);
    }
}
