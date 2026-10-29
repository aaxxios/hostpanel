using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetCustomerRetention;

/// <summary>
/// Validator for <see cref="GetCustomerRetentionQuery"/>. Its one input is a flag, which every
/// value of is valid, so no rule is declared: decided, not forgotten.
/// </summary>
public sealed class GetCustomerRetentionValidator : LocalizedValidator<GetCustomerRetentionQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetCustomerRetentionValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
