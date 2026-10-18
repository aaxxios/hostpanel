using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductFeatures;

/// <summary>Validates <see cref="GetProductFeaturesQuery"/> before it reaches the handler.</summary>
public sealed class GetProductFeaturesValidator : LocalizedValidator<GetProductFeaturesQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetProductFeaturesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.GroupId).GreaterThan(0).When(x => x.GroupId is not null)
            .WithMessage(L["ProductGroupIdInvalid"]);
        RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId is not null)
            .WithMessage(L["ProductIdInvalid"]);
    }
}
