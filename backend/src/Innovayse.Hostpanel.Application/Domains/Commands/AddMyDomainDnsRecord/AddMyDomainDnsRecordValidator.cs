using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.AddMyDomainDnsRecord;

/// <summary>Validates <see cref="AddMyDomainDnsRecordCommand"/> before it reaches the handler.</summary>
public sealed class AddMyDomainDnsRecordValidator : LocalizedValidator<AddMyDomainDnsRecordCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AddMyDomainDnsRecordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).MustBePositiveId(L["DomainIdRequired"]);
        RuleFor(x => x.Host).NotEmpty().WithMessage(L["DnsHostRequired"]);
        RuleFor(x => x.Value).NotEmpty().WithMessage(L["DnsValueRequired"]);
        RuleFor(x => x.Ttl).MustBePositiveId(L["DnsTtlPositive"]);
        RuleFor(x => x.Priority)
            .Must(p => p > 0)
            .When(x => x.Type is DnsRecordType.MX or DnsRecordType.SRV)
            .WithMessage(L["DnsPriorityRequiredForMxSrv"]);
    }
}
