using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetWhois;

/// <summary>Validates <see cref="GetWhoisQuery"/> inputs before the handler executes.</summary>
public sealed class GetWhoisValidator : LocalizedValidator<GetWhoisQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetWhoisValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainName).NotEmpty().WithMessage(L["DomainNameRequired"]);
    }
}
