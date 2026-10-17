using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Demo.Queries.GetDemo;

/// <summary>Validates <see cref="GetDemoQuery"/>: no rules — the query carries no input.</summary>
public sealed class GetDemoValidator : LocalizedValidator<GetDemoQuery>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetDemoValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
