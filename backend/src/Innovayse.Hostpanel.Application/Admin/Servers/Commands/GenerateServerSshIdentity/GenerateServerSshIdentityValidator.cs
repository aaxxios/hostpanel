using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.GenerateServerSshIdentity;

/// <summary>Shape checks for <see cref="GenerateServerSshIdentityCommand"/>.</summary>
public sealed class GenerateServerSshIdentityValidator : LocalizedValidator<GenerateServerSshIdentityCommand>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GenerateServerSshIdentityValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServerId).MustBePositiveId(L["ServerIdInvalid"]);
    }
}
