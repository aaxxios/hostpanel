using FluentValidation;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.ExportClientData;

/// <summary>
///     Validator for ExportClientDataCommand. An empty <see cref="ExportClientDataCommand.Fields"/> list is a
///     legitimate request for no sections at all, decided, not forgotten, but a key that names no
///     section is refused, so a misspelt key is a 400 instead of a silently missing section.
/// </summary>
public sealed class ExportClientDataValidator : LocalizedValidator<ExportClientDataCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ExportClientDataValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);

        // The sentence names the key that was refused, so one request with two typos reports both.
        RuleForEach(x => x.Fields)
            .Must(key => ClientExportSections.All.Contains(key))
            .WithMessage((_, key) => L["ClientExportFieldUnknown", key]);
    }
}
