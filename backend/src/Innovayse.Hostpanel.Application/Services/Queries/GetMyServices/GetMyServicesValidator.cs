using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetMyServices;

/// <summary>
/// Validates <see cref="GetMyServicesQuery"/>. The query carries no fields, since the account is
/// the caller's own; the validator exists so every use case has one.
/// </summary>
public sealed class GetMyServicesValidator : LocalizedValidator<GetMyServicesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetMyServicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
