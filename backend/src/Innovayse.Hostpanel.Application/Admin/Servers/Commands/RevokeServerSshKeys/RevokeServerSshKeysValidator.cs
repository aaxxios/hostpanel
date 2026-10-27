using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.RevokeServerSshKeys;

/// <summary>Shape checks for <see cref="RevokeServerSshKeysCommand"/>.</summary>
public sealed class RevokeServerSshKeysValidator : LocalizedValidator<RevokeServerSshKeysCommand>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public RevokeServerSshKeysValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServerId).MustBePositiveId(L["ServerIdInvalid"]);
    }
}
