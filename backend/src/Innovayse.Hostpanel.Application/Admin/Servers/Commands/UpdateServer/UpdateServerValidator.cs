using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.UpdateServer;

/// <summary>Validates <see cref="UpdateServerCommand"/> before it reaches the handler.</summary>
public sealed class UpdateServerValidator : LocalizedValidator<UpdateServerCommand>
{
    /// <summary>Longest a server name may be.</summary>
    private const int NameMaxLength = 200;

    /// <summary>Longest a hostname may be.</summary>
    private const int HostnameMaxLength = 253;

    /// <summary>Longest an API username may be.</summary>
    private const int UsernameMaxLength = 100;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateServerValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).MustBePositiveId(L["ServerIdInvalid"]);

        RuleFor(x => x.Name).NotEmpty().WithMessage(L["ServerNameRequired"])
            .MaximumLength(NameMaxLength).WithMessage(L["ServerNameTooLong", NameMaxLength]);

        RuleFor(x => x.Hostname).NotEmpty().WithMessage(L["ServerHostnameRequired"])
            .MaximumLength(HostnameMaxLength).WithMessage(L["ServerHostnameTooLong", HostnameMaxLength]);

        RuleFor(x => x.Username).NotEmpty().WithMessage(L["ServerUsernameRequired"])
            .MaximumLength(UsernameMaxLength).WithMessage(L["ServerUsernameTooLong", UsernameMaxLength]);

        RuleFor(x => x.MonthlyCost).GreaterThanOrEqualTo(0).WithMessage(L["ServerMonthlyCostInvalid"]);

        RuleFor(x => x.MaxAccounts).GreaterThan(0).When(x => x.MaxAccounts.HasValue)
            .WithMessage(L["ServerMaxAccountsInvalid"]);
    }
}
