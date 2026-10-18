using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateAnnouncement;

/// <summary>Validator for <see cref="UpdateAnnouncementCommand"/>.</summary>
public sealed class UpdateAnnouncementValidator : LocalizedValidator<UpdateAnnouncementCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateAnnouncementValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotNull().GreaterThan(0).WithMessage(L["AnnouncementIdInvalid"]);
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["AnnouncementTitleRequired"]);
        RuleFor(x => x.Content).NotEmpty().WithMessage(L["AnnouncementContentRequired"]);
    }
}
