using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetTicketFeedbackComments;

/// <summary>
/// Validator for <see cref="GetTicketFeedbackCommentsQuery"/>. The staff filter is a free-text
/// comparison and the date bounds are plain <c>WHERE</c> filters with no iteration behind them, so
/// nothing here can crash the handler; no rule is declared: decided, not forgotten.
/// </summary>
public sealed class GetTicketFeedbackCommentsValidator : LocalizedValidator<GetTicketFeedbackCommentsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetTicketFeedbackCommentsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
