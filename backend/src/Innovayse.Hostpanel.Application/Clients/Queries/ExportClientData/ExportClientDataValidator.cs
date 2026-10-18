using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Queries.ExportClientData;

/// <summary>
///     Validator for ExportClientDataQuery. <see cref="ExportClientDataQuery.Fields"/> has no rule:
///     an empty list is a legitimate request for no sections at all — decided, not forgotten.
/// </summary>
public sealed class ExportClientDataValidator : LocalizedValidator<ExportClientDataQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ExportClientDataValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
    }
}
