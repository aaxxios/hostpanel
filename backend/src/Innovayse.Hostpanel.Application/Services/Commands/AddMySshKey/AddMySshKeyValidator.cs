using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Application.Services.Common;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.AddMySshKey;

/// <summary>
/// Shape checks for <see cref="AddMySshKeyCommand"/>. Key validity itself is decided in the
/// handler, which answers with its own code so the portal can say exactly what was wrong.
/// </summary>
public sealed class AddMySshKeyValidator : LocalizedValidator<AddMySshKeyCommand>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public AddMySshKeyValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage(L["SshLabelRequired"])
            .MaximumLength(SshLimits.MaxLabelLength).WithMessage(L["SshLabelTooLong"]);
        RuleFor(x => x.PublicKey)
            .NotEmpty().WithMessage(L["SshPublicKeyRequired"])
            .MaximumLength(SshLimits.MaxPublicKeyLength).WithMessage(L["SshPublicKeyTooLong"]);
    }
}
