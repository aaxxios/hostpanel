using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProducts;

/// <summary>
///     Validator for GetProductsQuery.
///     <see cref="GetProductsQuery.Currency"/> is not checked here: any value, valid or not, is
///     handled gracefully by the handler, which falls back to the resolver's usual currency for an
///     unknown or disabled code rather than refusing the request.
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
