using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Products.Commands.UpdateProductFeature;

/// <summary>Validates <see cref="UpdateProductFeatureCommand"/> before it reaches the handler.</summary>
public sealed class UpdateProductFeatureValidator : LocalizedValidator<UpdateProductFeatureCommand>
{
    /// <summary>Longest a label may be — matches the <c>product_features.Label</c> column.</summary>
    private const int LabelMaxLength = 120;

    /// <summary>Longest a value may be — matches the <c>product_features.Value</c> column.</summary>
    private const int ValueMaxLength = 200;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateProductFeatureValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["ProductFeatureIdInvalid"]);
        RuleFor(x => x.Label).NotEmpty().WithMessage(L["ProductFeatureLabelRequired"])
            .MaximumLength(LabelMaxLength).WithMessage(L["ProductFeatureLabelTooLong", LabelMaxLength]);
        RuleFor(x => x.Value).NotEmpty().WithMessage(L["ProductFeatureValueRequired"])
            .MaximumLength(ValueMaxLength).WithMessage(L["ProductFeatureValueTooLong", ValueMaxLength]);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["ProductFeatureSortOrderInvalid"]);
    }
}
