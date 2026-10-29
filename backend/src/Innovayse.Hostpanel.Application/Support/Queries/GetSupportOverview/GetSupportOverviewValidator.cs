using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetSupportOverview;

/// <summary>
///     Validator for GetSupportOverviewQuery.
///     Requires the period to be a named member; the model binder also accepts a numeric value that is not one.
/// </summary>
public sealed class GetSupportOverviewValidator : LocalizedValidator<GetSupportOverviewQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetSupportOverviewValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Period)
            .IsInEnum()
            .WithMessage(L["TicketPeriodInvalid"]);
    }
}
