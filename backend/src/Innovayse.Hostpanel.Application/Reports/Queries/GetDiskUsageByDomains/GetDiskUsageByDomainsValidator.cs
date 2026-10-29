using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDiskUsageByDomains;

/// <summary>Validates <see cref="GetDiskUsageByDomainsQuery"/>: every domain must be named.</summary>
public sealed class GetDiskUsageByDomainsValidator : LocalizedValidator<GetDiskUsageByDomainsQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetDiskUsageByDomainsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.Domains).NotEmpty().WithMessage(L["ServiceDomainInvalid"]);
    }
}
