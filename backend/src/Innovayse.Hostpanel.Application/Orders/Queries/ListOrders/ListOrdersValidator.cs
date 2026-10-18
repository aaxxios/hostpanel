using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Orders.Queries.ListOrders;

/// <summary>
///     Validator for ListOrdersQuery. Page and page size are both clamped by the handler, and an
///     unrecognised status string is silently ignored there too, so no rule is declared for any of
///     them here — decided, not forgotten.
/// </summary>
public sealed class ListOrdersValidator : LocalizedValidator<ListOrdersQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListOrdersValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
