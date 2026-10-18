using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Orders.Commands.PlaceOrder;

/// <summary>Validates <see cref="PlaceOrderCommand"/> before it reaches the handler.</summary>
/// <remarks>
/// Registered Scoped, like every other validator (<c>Program.cs</c>'s
/// <c>AddValidatorsFromAssemblyContaining</c> call) — load-bearing here specifically, because this
/// validator injects the Scoped <see cref="ICurrentUserService"/> to decide whether the guest
/// registration fields are required. A Singleton registration would make this validator a captive
/// dependency serving every checkout the first caller's identity.
/// </remarks>
public sealed class PlaceOrderValidator : LocalizedValidator<PlaceOrderCommand>
{
    /// <summary>Length of an ISO 4217 alpha code, the only shape a currency may arrive in.</summary>
    private const int CurrencyCodeLength = 3;

    /// <summary>Initialises all validation rules for placing an order.</summary>
    /// <param name="caller">
    /// Who is checking out. The command no longer names a client, so whether the registration
    /// fields are required is a question about the credential rather than about the message.
    /// </param>
    /// <param name="localizer">String localizer for validation messages.</param>
    public PlaceOrderValidator(ICurrentUserService caller, IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage(L["PlaceOrderPaymentMethodRequired"]);
        RuleFor(x => x.Items).NotEmpty().WithMessage(L["PlaceOrderItemsRequired"]);

        // Shape only. Whether the code names a currency this panel offers is an I/O question,
        // and no validator here takes a repository; the handler asks it and refuses with the
        // matching error code.
        RuleFor(x => x.Currency)
            .Length(CurrencyCodeLength)
            .When(x => x.Currency is not null)
            .WithMessage(L["CurrencyCodeInvalid"]);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage(L["PlaceOrderItemProductIdInvalid"]);
            // The three spellings BillingCycleParser accepts. Anything else used to reach the
            // handler, where the parser's ArgumentException surfaced as a 500 instead of a 400.
            item.RuleFor(i => i.BillingCycle)
                .NotEmpty()
                .Must(c => c.ToLowerInvariant() is BillingCycleNames.Monthly or BillingCycleNames.Annual or BillingCycleNames.Annually)
                .WithMessage(L["PlaceOrderItemBillingCycleInvalid"]);
        });

        // Guest checkout is "no credential", not "no client id in the body". The handler
        // makes the same call, and it has to be the same call: a caller who could decide
        // which branch applied could place an order against an account that is not theirs.
        When(_ => caller.UserId is null, () =>
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage(L["PlaceOrderGuestFirstNameRequired"]);
            RuleFor(x => x.LastName).NotEmpty().WithMessage(L["PlaceOrderGuestLastNameRequired"]);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(L["PlaceOrderGuestEmailRequired"]);
            RuleFor(x => x.Password).NotEmpty().WithMessage(L["PlaceOrderGuestPasswordRequired"]);
        });
    }
}
