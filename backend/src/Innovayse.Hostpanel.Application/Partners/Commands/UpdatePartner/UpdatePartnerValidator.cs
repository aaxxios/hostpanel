using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Partners.Common;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Partners;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Partners.Commands.UpdatePartner;

/// <summary>Validates <see cref="UpdatePartnerCommand"/> before it reaches the handler.</summary>
public sealed class UpdatePartnerValidator : LocalizedValidator<UpdatePartnerCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdatePartnerValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["ContentIdInvalid"]);
        RuleFor(x => x.LogoUrl).MaximumLength(Partner.LogoUrlMaxLength).WithMessage(L["ContentUrlTooLong", Partner.LogoUrlMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.Url).MaximumLength(Partner.UrlMaxLength).WithMessage(L["ContentUrlTooLong", Partner.UrlMaxLength])
            .MustBeWebLink(L["ContentUrlInvalid"]);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).WithMessage(L["ContentSortOrderInvalid"]);
        RuleFor(x => x.Translations).NotEmpty().WithMessage(L["ContentTranslationsRequired"]);
        RuleForEach(x => x.Translations).SetValidator(new PartnerTranslationValidator(localizer));
    }
}
