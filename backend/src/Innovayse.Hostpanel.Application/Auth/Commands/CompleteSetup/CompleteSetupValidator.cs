using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.CompleteSetup;

/// <summary>
///     Validator for CompleteSetupCommand. Declares no rules: <see cref="CompleteSetupCommand.SetupToken"/>
///     is optional — absent under sso, and any string is a syntactically valid guess under local
///     mode — and whether it matches the one this installation issued is the handler's answer, not
///     a field rule. The absence was decided, not forgotten.
/// </summary>
public sealed class CompleteSetupValidator : LocalizedValidator<CompleteSetupCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CompleteSetupValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
