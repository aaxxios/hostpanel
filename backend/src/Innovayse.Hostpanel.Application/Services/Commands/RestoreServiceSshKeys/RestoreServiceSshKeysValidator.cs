using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.RestoreServiceSshKeys;

/// <summary>Validates <see cref="RestoreServiceSshKeysCommand"/> before the handler executes.</summary>
public sealed class RestoreServiceSshKeysValidator : LocalizedValidator<RestoreServiceSshKeysCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RestoreServiceSshKeysValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
    }
}
