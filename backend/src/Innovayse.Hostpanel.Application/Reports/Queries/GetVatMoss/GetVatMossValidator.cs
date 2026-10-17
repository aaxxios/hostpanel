using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetVatMoss;

/// <summary>
/// Validates <see cref="GetVatMossQuery"/> inputs before the handler executes. The unvalidated
/// endpoint silently mapped any quarter outside 1-4 onto Q4 (October-December); refusing it here
/// is the same fix the month-range validators elsewhere in this module already apply.
/// </summary>
public sealed class GetVatMossValidator : LocalizedValidator<GetVatMossQuery>
{
    /// <summary>Smallest valid calendar quarter.</summary>
    private const int MinQuarter = 1;

    /// <summary>Largest valid calendar quarter.</summary>
    private const int MaxQuarter = 4;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetVatMossValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Quarter!.Value)
            .InclusiveBetween(MinQuarter, MaxQuarter)
            .When(x => x.Quarter.HasValue)
            .WithMessage(L["ReportQuarterInvalid"]);
    }
}
