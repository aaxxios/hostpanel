using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListPublicCurrencies;

/// <summary>
///     Validator for <see cref="ListPublicCurrenciesQuery"/>. The query carries nothing the caller chooses, so
///     no rule is declared here; decided, not forgotten.
/// </summary>
public sealed class ListPublicCurrenciesValidator : LocalizedValidator<ListPublicCurrenciesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListPublicCurrenciesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
