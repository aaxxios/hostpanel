using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyContact;

/// <summary>
/// Validator for <see cref="RemoveMyContactCommand"/>. Declares no rules: the handler forwards the
/// id to <c>RemoveContactCommand</c>, whose validator owns the id rule and which only runs once the
/// caller's profile has been found -- so a caller with no profile still answers 404.
/// </summary>
public sealed class RemoveMyContactValidator : LocalizedValidator<RemoveMyContactCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RemoveMyContactValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
