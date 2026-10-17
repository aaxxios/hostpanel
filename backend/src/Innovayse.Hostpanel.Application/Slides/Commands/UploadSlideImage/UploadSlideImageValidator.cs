using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UploadSlideImage;

/// <summary>
///     Validates <see cref="UploadSlideImageCommand"/> before it reaches the handler.
///     Only the shape is checked here — whether the media type and size are acceptable is the
///     handler's answer, because those are ceilings this deployment enforces, not a malformed
///     request.
/// </summary>
public sealed class UploadSlideImageValidator : LocalizedValidator<UploadSlideImageCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UploadSlideImageValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Length).GreaterThan(0).WithMessage(L["SlideImageRequired"]);
        RuleFor(x => x.ContentType).NotEmpty().WithMessage(L["SlideImageRequired"]);
    }
}
