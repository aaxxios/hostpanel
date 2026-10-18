using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Users.Queries.GetUser;

/// <summary>
///     Validates <see cref="GetUserQuery"/>. Its only field is the route-supplied subject, so the
///     single rule guards against an empty id; there is deliberately nothing further to check.
/// </summary>
public sealed class GetUserValidator : LocalizedValidator<GetUserQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetUserValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(L["UserIdRequired"]);
    }
}
