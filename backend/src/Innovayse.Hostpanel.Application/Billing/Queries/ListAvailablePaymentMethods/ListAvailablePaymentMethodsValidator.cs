using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Queries.ListAvailablePaymentMethods;

/// <summary>
///     Validator for <see cref="ListAvailablePaymentMethodsQuery"/>. Declares no rules: any
///     string is a syntactically valid ISO 4217 code to try, and a code the panel does not offer
///     is answered with an empty list by the handler rather than refused — the decision this
///     validator's emptiness records.
/// </summary>
public sealed class ListAvailablePaymentMethodsValidator : LocalizedValidator<ListAvailablePaymentMethodsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListAvailablePaymentMethodsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
