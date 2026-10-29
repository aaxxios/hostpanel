using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListEndOfCycleTerminationsDue;

/// <summary>
/// Validates <see cref="ListEndOfCycleTerminationsDueQuery"/>: no rules, any moment is a valid cut-off
/// (an unset one simply finds nothing due).
/// </summary>
public sealed class ListEndOfCycleTerminationsDueValidator : LocalizedValidator<ListEndOfCycleTerminationsDueQuery>
{
    /// <summary>Initializes the validator.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ListEndOfCycleTerminationsDueValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
