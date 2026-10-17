using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDailyTransactions;

/// <summary>Validates <see cref="GetDailyTransactionsQuery"/> inputs before the handler executes.</summary>
public sealed class GetDailyTransactionsValidator : LocalizedValidator<GetDailyTransactionsQuery>
{
    /// <summary>Smallest valid calendar month.</summary>
    private const int MinMonth = 1;

    /// <summary>Largest valid calendar month.</summary>
    private const int MaxMonth = 12;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetDailyTransactionsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Month!.Value)
            .InclusiveBetween(MinMonth, MaxMonth)
            .When(x => x.Month.HasValue)
            .WithMessage(L["ReportMonthInvalid"]);
    }
}
