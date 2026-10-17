using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.AdminCreateClient;

/// <summary>
/// Validates <see cref="AdminCreateClientCommand"/> before the handler executes. Applies
/// conditional rules based on whether a new user is being created or an existing one is linked.
/// </summary>
public sealed class AdminCreateClientValidator : LocalizedValidator<AdminCreateClientCommand>
{
    /// <summary>Shortest password this panel will provision for a new account.</summary>
    private const int MinimumPasswordLength = 8;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AdminCreateClientValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        When(x => x.CreateNewUser, () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(L["ClientNewUserEmailInvalid"]);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(MinimumPasswordLength)
                .WithMessage(L["ClientNewUserPasswordInvalid", MinimumPasswordLength]);
        });

        When(x => !x.CreateNewUser, () =>
        {
            RuleFor(x => x.ExistingUserId).NotEmpty().WithMessage(L["ClientUserIdRequired"]);
        });

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage(L["ClientFirstNameInvalid"]);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage(L["ClientLastNameInvalid"]);
        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage(L["ClientCompanyNameInvalid"])
            .When(x => x.CompanyName is not null);
        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage(L["ClientPhoneInvalid"])
            .When(x => x.Phone is not null);

        // Guarded on blank, not on null: the add-client screen sends `country: value || undefined`
        // but other callers send "", and an empty string is "not provided", not a two-character
        // code that happens to be zero characters long.
        RuleFor(x => x.Country)
            .Length(2).WithMessage(L["ClientCountryInvalid"])
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.Currency)
            .Length(3).WithMessage(L["ClientCurrencyInvalid"])
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.AdminNotes)
            .MaximumLength(2000).WithMessage(L["ClientAdminNotesInvalid"])
            .When(x => x.AdminNotes is not null);

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage(L["ClientPaymentMethodInvalid"])
            .When(x => x.PaymentMethod is not null);

        RuleFor(x => x.BillingContact)
            .MaximumLength(256).WithMessage(L["ClientBillingContactInvalid"])
            .When(x => x.BillingContact is not null);
    }
}
