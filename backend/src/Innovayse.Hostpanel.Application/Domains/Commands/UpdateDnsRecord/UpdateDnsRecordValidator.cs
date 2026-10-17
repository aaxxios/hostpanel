using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateDnsRecord;

/// <summary>Validates <see cref="UpdateDnsRecordCommand"/> inputs before the handler executes.</summary>
public sealed class UpdateDnsRecordValidator : LocalizedValidator<UpdateDnsRecordCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateDnsRecordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
        RuleFor(x => x.RecordId).GreaterThan(0).WithMessage(L["DnsRecordIdRequired"]);
        RuleFor(x => x.Value).NotEmpty().WithMessage(L["DnsValueRequired"]);
        RuleFor(x => x.Ttl).MustBePositiveId(L["DnsTtlPositive"]);
    }
}
