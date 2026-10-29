using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Queries.GetContactNotificationEmails;

/// <summary>Validates <see cref="GetContactNotificationEmailsQuery"/> before it reaches the handler.</summary>
public sealed class GetContactNotificationEmailsValidator : LocalizedValidator<GetContactNotificationEmailsQuery>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetContactNotificationEmailsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.Topic).IsInEnum().WithMessage(L["ContactNotificationTopicInvalid"]);
    }
}
