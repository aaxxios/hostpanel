using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.RevokeMySshKey;

/// <summary>Shape checks for <see cref="RevokeMySshKeyCommand"/>.</summary>
public sealed class RevokeMySshKeyValidator : LocalizedValidator<RevokeMySshKeyCommand>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public RevokeMySshKeyValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.KeyId).NotEmpty().WithMessage(L["SshKeyIdInvalid"]);
    }
}
