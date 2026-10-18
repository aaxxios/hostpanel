using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetClientTicketStats;

/// <summary>Validator for <see cref="GetClientTicketStatsQuery"/>.</summary>
public sealed class GetClientTicketStatsValidator : LocalizedValidator<GetClientTicketStatsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetClientTicketStatsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage(L["TicketClientIdInvalid"]);
    }
}
