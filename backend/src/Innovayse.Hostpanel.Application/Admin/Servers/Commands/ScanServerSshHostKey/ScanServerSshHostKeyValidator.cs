using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.ScanServerSshHostKey;

/// <summary>Shape checks for <see cref="ScanServerSshHostKeyCommand"/>.</summary>
public sealed class ScanServerSshHostKeyValidator : LocalizedValidator<ScanServerSshHostKeyCommand>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ScanServerSshHostKeyValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServerId).MustBePositiveId(L["ServerIdInvalid"]);
    }
}
