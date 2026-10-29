using FluentValidation;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceHostingAccount;

/// <summary>Validates <see cref="UpdateServiceHostingAccountCommand"/> before the handler executes.</summary>
public sealed class UpdateServiceHostingAccountValidator : LocalizedValidator<UpdateServiceHostingAccountCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateServiceHostingAccountValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.ServerId).GreaterThan(0).WithMessage(L["HostingAccountServerIdInvalid"])
            .When(x => x.ServerId is not null);
        RuleFor(x => x.Domain).MaximumLength(253).WithMessage(L["HostingAccountDomainTooLong"])
            .When(x => x.Domain is not null);
        RuleFor(x => x.ProvisioningRef).MaximumLength(256).WithMessage(L["HostingAccountProvisioningRefTooLong"])
            .When(x => x.ProvisioningRef is not null);
        RuleFor(x => x.DedicatedIp).MaximumLength(45).WithMessage(L["HostingAccountDedicatedIpTooLong"])
            .When(x => x.DedicatedIp is not null);
        RuleFor(x => x.Username).MaximumLength(100).WithMessage(L["HostingAccountUsernameTooLong"])
            .When(x => x.Username is not null);
        RuleFor(x => x.Password).MaximumLength(ProtectedValueLimits.ServicePassword).WithMessage(L["HostingAccountPasswordTooLong"])
            .When(x => x.Password is not null);
    }
}
