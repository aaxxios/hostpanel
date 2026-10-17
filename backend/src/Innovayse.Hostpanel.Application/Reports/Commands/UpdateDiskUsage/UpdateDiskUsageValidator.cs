using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Commands.UpdateDiskUsage;

/// <summary>
/// Validator for <see cref="UpdateDiskUsageCommand"/>. The command carries no input, so there is
/// deliberately nothing to check here; the class exists so the use case is a complete set and the
/// next reader knows the absence of rules was decided rather than forgotten.
/// </summary>
public sealed class UpdateDiskUsageValidator : LocalizedValidator<UpdateDiskUsageCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateDiskUsageValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
