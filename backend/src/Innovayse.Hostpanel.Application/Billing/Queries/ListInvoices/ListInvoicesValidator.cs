using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Queries.ListInvoices;

/// <summary>
///     Validator for ListInvoicesQuery. Page and page size are both clamped by the handler, so no
///     rule is declared for them here — decided, not forgotten.
/// </summary>
public sealed class ListInvoicesValidator : LocalizedValidator<ListInvoicesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListInvoicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
