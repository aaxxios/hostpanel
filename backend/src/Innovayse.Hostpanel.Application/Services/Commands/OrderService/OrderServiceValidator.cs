using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Services.Commands.OrderService;
/// <summary>Validates <see cref="OrderServiceCommand"/> before the handler executes.</summary>
public sealed class OrderServiceValidator : LocalizedValidator<OrderServiceCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public OrderServiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["OrderServiceClientIdInvalid"]);
        RuleFor(x => x.ProductId).MustBePositiveId(L["OrderServiceProductIdInvalid"]);
        // Matched to what the pipeline actually produces, not to the two words the doc comment
        // names. PlaceOrderHandler.ResolvePrice accepts "monthly", "annual" and "annually", in any
        // case, and stores the caller's spelling on the order item; AcceptOrderHandler and
        // FulfillPaidOrderHandler then re-dispatch that stored string as this command. A rule that
        // took only the lower-case first two would have refused a paid order at provisioning time.
        RuleFor(x => x.BillingCycle)
            .NotEmpty()
            .Must(c => c.ToLowerInvariant() is BillingCycleNames.Monthly or BillingCycleNames.Annual or BillingCycleNames.Annually)
            .WithMessage(L["OrderServiceBillingCycleInvalid"]);
        RuleFor(x => x.FirstPaymentAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L["OrderServiceAmountInvalid"])
            .When(x => x.FirstPaymentAmount is not null);
        RuleFor(x => x.RecurringAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L["OrderServiceAmountInvalid"])
            .When(x => x.RecurringAmount is not null);
    }
}
