using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Products.Queries.ListCatalogProducts;

/// <summary>
///     Validator for <see cref="ListCatalogProductsQuery"/>.
///     <see cref="ListCatalogProductsQuery.Currency"/> is not checked here: any value, valid or not,
///     is handled by the handler, which falls back to the resolver's usual currency for an unknown
///     or disabled code rather than refusing the request.
/// </summary>
public sealed class ListCatalogProductsValidator : LocalizedValidator<ListCatalogProductsQuery>
{
    /// <summary>The most groups one request may name.</summary>
    private const int MaxGroupIds = 50;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListCatalogProductsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.GroupId).GreaterThan(0).When(x => x.GroupId is not null)
            .WithMessage(L["ProductGroupIdInvalid"]);
        RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId is not null)
            .WithMessage(L["ProductIdInvalid"]);

        // Bounded: the ids become a SQL IN list, and nobody lists more groups than a store has.
        RuleFor(x => x.GroupIds)
            .Must(ids => ids is null || (ids.Length <= MaxGroupIds && ids.All(id => id > 0)))
            .WithMessage(L["ProductGroupIdInvalid"]);
    }
}
