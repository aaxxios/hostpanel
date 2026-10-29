using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;

/// <summary>
/// Validator for <see cref="SyncTldCostPricesCommand"/>.
/// </summary>
/// <remarks>
/// No rule: the command carries no parameters, so there is no shape of input this validator could
/// refuse. It still exists and still derives from <see cref="LocalizedValidator{T}"/>: the
/// absence of a rule here is decided, not forgotten.
/// </remarks>
public sealed class SyncTldCostPricesValidator : LocalizedValidator<SyncTldCostPricesCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SyncTldCostPricesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
