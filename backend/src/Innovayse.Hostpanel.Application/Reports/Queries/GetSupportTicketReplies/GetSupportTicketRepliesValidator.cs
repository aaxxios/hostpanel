using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSupportTicketReplies;

/// <summary>Validates <see cref="GetSupportTicketRepliesQuery"/> inputs before the handler executes.</summary>
public sealed class GetSupportTicketRepliesValidator : LocalizedValidator<GetSupportTicketRepliesQuery>
{
    /// <summary>Smallest valid calendar month.</summary>
    private const int MinMonth = 1;

    /// <summary>Largest valid calendar month.</summary>
    private const int MaxMonth = 12;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetSupportTicketRepliesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Month!.Value)
            .InclusiveBetween(MinMonth, MaxMonth)
            .When(x => x.Month.HasValue)
            .WithMessage(L["ReportMonthInvalid"]);
    }
}
