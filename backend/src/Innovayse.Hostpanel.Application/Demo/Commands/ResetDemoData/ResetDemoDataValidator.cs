using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Demo.Commands.ResetDemoData;

/// <summary>Validates <see cref="ResetDemoDataCommand"/>: no rules — the command carries no input.</summary>
public sealed class ResetDemoDataValidator : LocalizedValidator<ResetDemoDataCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ResetDemoDataValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
