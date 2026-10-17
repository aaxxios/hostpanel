using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetServices;
/// <summary>Validates <see cref="GetServicesQuery"/> before the handler executes.</summary>
public sealed class GetServicesValidator : LocalizedValidator<GetServicesQuery>
{
    /// <summary>Most rows a page may hold.</summary>
    private const int MaxPageSize = 100;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetServicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Page).MustBePage(L["ServicePageInvalid"]);
        RuleFor(x => x.PageSize).MustBePageSize(MaxPageSize, L["ServicePageSizeInvalid"]);
        RuleFor(x => x.ClientId).GreaterThan(0).WithMessage(L["ServiceClientIdFilterInvalid"])
            .When(x => x.ClientId is not null);
    }
}
