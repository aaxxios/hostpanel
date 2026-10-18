using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Queries.GetClientGrowthReport;

/// <summary>
///     Validates <see cref="GetClientGrowthReportQuery"/>. Any two dates are accepted, including a
///     start after the end — the handler answers an empty report rather than refusing it —
///     decided, not forgotten.
/// </summary>
public sealed class GetClientGrowthReportValidator : LocalizedValidator<GetClientGrowthReportQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetClientGrowthReportValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
