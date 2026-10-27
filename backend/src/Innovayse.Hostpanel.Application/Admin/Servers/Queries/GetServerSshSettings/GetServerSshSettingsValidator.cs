using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Queries.GetServerSshSettings;

/// <summary>Shape checks for <see cref="GetServerSshSettingsQuery"/>.</summary>
public sealed class GetServerSshSettingsValidator : LocalizedValidator<GetServerSshSettingsQuery>
{
    /// <summary>Initialises the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetServerSshSettingsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServerId).MustBePositiveId(L["ServerIdInvalid"]);
    }
}
