using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateMyDomainDnsRecord;

/// <summary>Validates <see cref="UpdateMyDomainDnsRecordCommand"/> before it reaches the handler.</summary>
public sealed class UpdateMyDomainDnsRecordValidator : LocalizedValidator<UpdateMyDomainDnsRecordCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateMyDomainDnsRecordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).MustBePositiveId(L["DomainIdRequired"]);
        RuleFor(x => x.RecordId).MustBePositiveId(L["DnsRecordIdRequired"]);
        RuleFor(x => x.Value).NotEmpty().WithMessage(L["DnsValueRequired"]);
        RuleFor(x => x.Ttl).MustBePositiveId(L["DnsTtlPositive"]);
    }
}
