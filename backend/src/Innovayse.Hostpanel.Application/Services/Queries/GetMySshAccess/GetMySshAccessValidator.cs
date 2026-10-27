using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetMySshAccess;

/// <summary>Shape checks for <see cref="GetMySshAccessQuery"/>.</summary>
public sealed class GetMySshAccessValidator : LocalizedValidator<GetMySshAccessQuery>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetMySshAccessValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
    }
}
