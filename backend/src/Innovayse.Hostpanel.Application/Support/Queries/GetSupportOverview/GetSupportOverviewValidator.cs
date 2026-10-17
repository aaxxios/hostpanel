using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetSupportOverview;

/// <summary>
///     Validator for GetSupportOverviewQuery.
///     Only requires the period to be present; an unrecognised value is handled by the handler,
///     which falls back to "today" rather than refusing — unchanged from the previous behaviour.
/// </summary>
public sealed class GetSupportOverviewValidator : LocalizedValidator<GetSupportOverviewQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetSupportOverviewValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Period)
            .NotEmpty()
            .WithMessage(L["TicketPeriodRequired"]);
    }
}
