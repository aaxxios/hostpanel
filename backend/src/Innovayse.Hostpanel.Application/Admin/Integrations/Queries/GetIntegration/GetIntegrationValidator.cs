using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Integrations.Queries.GetIntegration;

/// <summary>Validates <see cref="GetIntegrationQuery"/> before it reaches the handler.</summary>
public sealed class GetIntegrationValidator : LocalizedValidator<GetIntegrationQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetIntegrationValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage(L["IntegrationSlugRequired"]);
    }
}
