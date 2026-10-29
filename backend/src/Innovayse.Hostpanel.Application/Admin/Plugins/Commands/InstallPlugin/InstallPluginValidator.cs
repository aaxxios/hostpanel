using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Commands.InstallPlugin;

/// <summary>
///     Validates <see cref="InstallPluginCommand"/> before it reaches the handler.
///     Only that a file was sent is checked here; whether it is a well-formed plugin (an empty
///     file included) is the handler's answer.
/// </summary>
public sealed class InstallPluginValidator : LocalizedValidator<InstallPluginCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public InstallPluginValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Content).NotNull().WithMessage(L["PluginZipRequired"]);
    }
}
