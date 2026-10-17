using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateAnnouncement;

/// <summary>Validator for <see cref="CreateAnnouncementCommand"/>.</summary>
public sealed class CreateAnnouncementValidator : LocalizedValidator<CreateAnnouncementCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateAnnouncementValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["AnnouncementTitleRequired"]);
        RuleFor(x => x.Content).NotEmpty().WithMessage(L["AnnouncementContentRequired"]);
    }
}
