using FluentValidation;
using Innovayse.Hostpanel.Application.Admin.Integrations.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Commands.SaveIntegrationConfig;

/// <summary>
/// Validates <see cref="SaveIntegrationConfigCommand"/> before it reaches the handler. Every field
/// key in <see cref="SaveIntegrationConfigCommand.Config"/> and
/// <see cref="SaveIntegrationConfigCommand.ClearFields"/> must be one the integration declares, so
/// a save cannot plant arbitrary <c>integration:*</c> rows. A slug that names no integration is
/// left to the handler, which answers <c>INTEGRATION_NOT_FOUND</c>.
/// </summary>
public sealed class SaveIntegrationConfigValidator : LocalizedValidator<SaveIntegrationConfigCommand>
{
    /// <summary>Loaded plugins, whose manifests declare the fields of a plugin integration.</summary>
    private readonly IPluginRegistry _plugins;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    /// <param name="plugins">Loaded plugins, whose manifests declare the fields of a plugin integration.</param>
    public SaveIntegrationConfigValidator(IStringLocalizer<ApplicationValidationMessages> localizer, IPluginRegistry plugins)
        : base(localizer)
    {
        _plugins = plugins;

        RuleFor(x => x.Slug).NotEmpty().WithMessage(L["IntegrationSlugRequired"]);
        RuleFor(x => x.Config).NotNull().WithMessage(L["IntegrationConfigRequired"]);
        RuleFor(x => x)
            .Must(OnlyDeclaredFields)
            .WithName(nameof(SaveIntegrationConfigCommand.Config))
            .WithMessage(L["IntegrationFieldUnknown"])
            .When(x => !string.IsNullOrEmpty(x.Slug) && x.Config is not null);
        RuleFor(x => x.Config)
            .Must(config => config.Values.All(v => v is null || v.Length <= ProtectedValueLimits.SettingValue))
            .WithMessage(L["SettingValueTooLong", ProtectedValueLimits.SettingValue])
            .When(x => x.Config is not null);
    }

    /// <summary>Tells whether every written or cleared field is declared by the integration.</summary>
    /// <param name="command">The command under validation.</param>
    /// <returns><c>true</c> when all keys are declared, or when the slug is unknown (refused by the handler).</returns>
    private bool OnlyDeclaredFields(SaveIntegrationConfigCommand command)
    {
        var definition = IntegrationFields.Resolve(command.Slug, _plugins);
        if (definition is null)
        {
            return true;
        }

        var declared = definition.Fields.Select(f => f.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return command.Config.Keys.All(declared.Contains)
            && (command.ClearFields ?? []).All(declared.Contains);
    }
}
