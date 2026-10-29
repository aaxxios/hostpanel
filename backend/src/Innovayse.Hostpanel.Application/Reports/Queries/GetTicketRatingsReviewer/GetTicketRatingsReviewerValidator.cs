using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetTicketRatingsReviewer;

/// <summary>
/// Validator for <see cref="GetTicketRatingsReviewerQuery"/>. Every field is a plain <c>WHERE</c>
/// filter with no iteration behind it, an out-of-range minimum rating simply matches nothing, so
/// nothing here can crash the handler; no rule is declared, decided, not forgotten.
/// </summary>
public sealed class GetTicketRatingsReviewerValidator : LocalizedValidator<GetTicketRatingsReviewerQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetTicketRatingsReviewerValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
