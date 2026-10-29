using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Orders.Queries.ListOrders;

/// <summary>
///     Validator for ListOrdersQuery. Page and page size are both clamped by the handler, and the
///     status is typed, so an unknown value is refused at binding; no rule is declared here.
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
