using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetTicketFeedbackScores;

/// <summary>
/// Validator for <see cref="GetTicketFeedbackScoresQuery"/>. Both bounds are plain <c>WHERE</c>
/// filters with no iteration behind them, so nothing here can crash the handler; no rule is
/// declared — decided, not forgotten.
/// </summary>
public sealed class GetTicketFeedbackScoresValidator : LocalizedValidator<GetTicketFeedbackScoresQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetTicketFeedbackScoresValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
