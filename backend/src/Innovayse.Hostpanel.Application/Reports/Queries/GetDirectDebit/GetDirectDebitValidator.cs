using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDirectDebit;

/// <summary>
/// Validator for <see cref="GetDirectDebitQuery"/>. The query carries no input, so it declares no
/// rules; the absence was decided, not forgotten.
/// </summary>
public sealed class GetDirectDebitValidator : LocalizedValidator<GetDirectDebitQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetDirectDebitValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
