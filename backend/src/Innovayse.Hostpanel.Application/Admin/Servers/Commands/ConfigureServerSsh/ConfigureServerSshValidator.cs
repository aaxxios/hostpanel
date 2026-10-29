using FluentValidation;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.ConfigureServerSsh;

/// <summary>Shape checks for <see cref="ConfigureServerSshCommand"/>.</summary>
public sealed class ConfigureServerSshValidator : LocalizedValidator<ConfigureServerSshCommand>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ConfigureServerSshValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServerId).MustBePositiveId(L["ServerIdInvalid"]);
        RuleFor(x => x.SshHostname).NotNull().MaximumLength(255).Matches(@"^[A-Za-z0-9.\-]*$").WithMessage(L["SshHostnameInvalid"]);
        RuleFor(x => x.AgentHost).NotNull().MaximumLength(255).Matches(@"^[A-Za-z0-9.\-:]*$").WithMessage(L["SshHostnameInvalid"]);
        RuleFor(x => x.AgentUsername).NotNull().MaximumLength(32).Matches("^([a-z_][a-z0-9_-]*)?$").WithMessage(L["SshAgentUsernameInvalid"]);
        RuleFor(x => x.SshPort).InclusiveBetween(1, 65535).WithMessage(L["SshPortInvalid"]);
        RuleFor(x => x.AgentPort).InclusiveBetween(1, 65535).WithMessage(L["SshPortInvalid"]);
        RuleFor(x => x.AgentHostKeyFingerprint).NotNull().Matches("^(SHA256:[A-Za-z0-9+/]{43})?$").WithMessage(L["SshHostKeyFingerprintInvalid"]);
        RuleFor(x => x.AgentPrivateKey).MaximumLength(ProtectedValueLimits.AgentPrivateKey).WithMessage(L["SshAgentPrivateKeyTooLong"]);
        RuleFor(x => x.ConnectTimeoutSeconds).InclusiveBetween(1, 120).WithMessage(L["SshTimeoutInvalid"]);
        RuleFor(x => x.OperationTimeoutSeconds).InclusiveBetween(1, 300).WithMessage(L["SshTimeoutInvalid"]);
    }
}
