using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.DeleteDnsRecord;

/// <summary>Validates <see cref="DeleteDnsRecordCommand"/> inputs before the handler executes.</summary>
public sealed class DeleteDnsRecordValidator : LocalizedValidator<DeleteDnsRecordCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DeleteDnsRecordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
        RuleFor(x => x.RecordId).GreaterThan(0).WithMessage(L["DnsRecordIdRequired"]);
    }
}
