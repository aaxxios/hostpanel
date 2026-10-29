using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoices;

/// <summary>
///     Validator for <see cref="GetMyInvoicesQuery"/>. The query carries nothing the caller chooses, so
///     no rule is declared here; decided, not forgotten.
/// </summary>
public sealed class GetMyInvoicesValidator : LocalizedValidator<GetMyInvoicesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetMyInvoicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
