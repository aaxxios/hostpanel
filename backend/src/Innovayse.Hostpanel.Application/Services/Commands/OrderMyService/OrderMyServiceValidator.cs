using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.OrderMyService;

/// <summary>Validates <see cref="OrderMyServiceCommand"/> before the handler executes.</summary>
public sealed class OrderMyServiceValidator : LocalizedValidator<OrderMyServiceCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public OrderMyServiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ProductId).MustBePositiveId(L["OrderServiceProductIdInvalid"]);
        RuleFor(x => x.BillingCycle)
            .NotEmpty()
            .Must(c => c.ToLowerInvariant() is BillingCycleNames.Monthly or BillingCycleNames.Annual or BillingCycleNames.Annually)
            .WithMessage(L["OrderServiceBillingCycleInvalid"]);
    }
}
