using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProducts;

/// <summary>
///     Validator for GetProductsQuery: a group filter, when given, names a real id shape.
/// </summary>
public sealed class GetProductsValidator : LocalizedValidator<GetProductsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetProductsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.GroupId).GreaterThan(0).When(x => x.GroupId is not null)
            .WithMessage(L["ProductGroupIdInvalid"]);
    }
}
