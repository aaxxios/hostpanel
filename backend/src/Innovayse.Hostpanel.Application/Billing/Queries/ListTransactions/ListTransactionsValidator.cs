using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Queries.ListTransactions;

/// <summary>
///     Validator for ListTransactionsQuery. Page and page size carry safe defaults and the handler
///     does not clamp them further, so no rule is declared here — decided, not forgotten.
/// </summary>
public sealed class ListTransactionsValidator : LocalizedValidator<ListTransactionsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListTransactionsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
