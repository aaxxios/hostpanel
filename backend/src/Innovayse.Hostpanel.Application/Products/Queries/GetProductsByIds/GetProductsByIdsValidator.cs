using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductsByIds;

/// <summary>Validates <see cref="GetProductsByIdsQuery"/> before it reaches the handler.</summary>
public sealed class GetProductsByIdsValidator : LocalizedValidator<GetProductsByIdsQuery>
{
    /// <summary>Initializes the validator with the localized messages.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetProductsByIdsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.ProductIds).GreaterThan(0).WithMessage(L["ProductIdInvalid"]);
    }
}
