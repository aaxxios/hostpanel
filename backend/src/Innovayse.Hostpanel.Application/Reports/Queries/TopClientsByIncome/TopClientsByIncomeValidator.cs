using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.TopClientsByIncome;

/// <summary>
/// Validates <see cref="TopClientsByIncomeQuery"/> inputs before the handler executes. The
/// unvalidated endpoint let <c>take</c> reach the database's <c>LIMIT</c> clause as zero or
/// negative, so the bound here also protects the query rather than just the response size.
/// </summary>
public sealed class TopClientsByIncomeValidator : LocalizedValidator<TopClientsByIncomeQuery>
{
    /// <summary>Largest number of clients a single request may rank.</summary>
    private const int MaxTake = 100;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public TopClientsByIncomeValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Take).InclusiveBetween(1, MaxTake).WithMessage(L["ReportTakeInvalid"]);
    }
}
