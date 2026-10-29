using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceNotes;

/// <summary>Validates <see cref="UpdateServiceNotesCommand"/> before the handler executes.</summary>
public sealed class UpdateServiceNotesValidator : LocalizedValidator<UpdateServiceNotesCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateServiceNotesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.AdminNotes).MaximumLength(2000).WithMessage(L["ServiceAdminNotesInvalid"])
            .When(x => x.AdminNotes is not null);
    }
}
