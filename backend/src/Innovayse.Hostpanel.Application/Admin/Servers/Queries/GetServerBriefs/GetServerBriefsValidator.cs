using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerBriefs;

/// <summary>Validates <see cref="GetServerBriefsQuery"/> before it reaches the handler.</summary>
public sealed class GetServerBriefsValidator : LocalizedValidator<GetServerBriefsQuery>
{
    /// <summary>Initializes the validator with the localized messages.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetServerBriefsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.ServerIds).GreaterThan(0).WithMessage(L["ServerIdInvalid"]);
    }
}
