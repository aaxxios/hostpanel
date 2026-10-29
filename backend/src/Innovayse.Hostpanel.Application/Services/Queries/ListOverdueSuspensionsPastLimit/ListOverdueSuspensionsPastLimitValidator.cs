using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListOverdueSuspensionsPastLimit;

/// <summary>
/// Validates <see cref="ListOverdueSuspensionsPastLimitQuery"/>: no rules, any moment is a valid cut-off
/// (an unset one simply finds nothing past the limit).
/// </summary>
public sealed class ListOverdueSuspensionsPastLimitValidator : LocalizedValidator<ListOverdueSuspensionsPastLimitQuery>
{
    /// <summary>Initializes the validator.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ListOverdueSuspensionsPastLimitValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
