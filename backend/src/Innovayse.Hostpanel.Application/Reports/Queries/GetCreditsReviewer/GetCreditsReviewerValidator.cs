using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetCreditsReviewer;

/// <summary>Validates <see cref="GetCreditsReviewerQuery"/> inputs before the handler executes.</summary>
public sealed class GetCreditsReviewerValidator : LocalizedValidator<GetCreditsReviewerQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetCreditsReviewerValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId!.Value)
            .GreaterThan(0)
            .When(x => x.ClientId.HasValue)
            .WithMessage(L["ReportClientIdInvalid"]);
    }
}
