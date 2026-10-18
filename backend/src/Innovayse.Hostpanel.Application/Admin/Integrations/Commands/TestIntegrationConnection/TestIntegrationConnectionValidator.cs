using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Commands.TestIntegrationConnection;

/// <summary>Validates <see cref="TestIntegrationConnectionCommand"/> before it reaches the handler.</summary>
public sealed class TestIntegrationConnectionValidator : LocalizedValidator<TestIntegrationConnectionCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public TestIntegrationConnectionValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage(L["IntegrationSlugRequired"]);
    }
}
