using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetClientBriefs;

/// <summary>Validates <see cref="GetClientBriefsQuery"/> before it reaches the handler.</summary>
public sealed class GetClientBriefsValidator : LocalizedValidator<GetClientBriefsQuery>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetClientBriefsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.ClientIds).GreaterThan(0).WithMessage(L["ClientIdInvalid"]);
        RuleForEach(x => x.UserSubjects).NotEmpty().WithMessage(L["UserSubjectInvalid"]);
    }
}
