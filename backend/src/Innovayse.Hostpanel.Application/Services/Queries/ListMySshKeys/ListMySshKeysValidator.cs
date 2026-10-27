using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListMySshKeys;

/// <summary>Shape checks for <see cref="ListMySshKeysQuery"/>.</summary>
public sealed class ListMySshKeysValidator : LocalizedValidator<ListMySshKeysQuery>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ListMySshKeysValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
    }
}
