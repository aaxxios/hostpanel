using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.SendContactMessage;

/// <summary>
/// Validator for <see cref="SendContactMessageCommand"/>. The endpoint is anonymous and reachable
/// without the page, so these caps are enforced here rather than left to the form's own
/// <c>maxlength</c> attributes.
/// </summary>
/// <remarks>
/// Whether this deployment has a configured enquiry inbox, and whether the relay accepts the
/// message, are not field-shape rules and stay the handler's own refusals
/// (<c>CONTACT_NOT_CONFIGURED</c>, <c>CONTACT_SEND_FAILED</c>): a validator can only answer 400
/// <c>VALIDATION_ERROR</c>, and collapsing "you typed something wrong" into "this deployment
/// cannot accept contact mail" would lose a distinction the client BFF branches on.
/// </remarks>
public sealed class SendContactMessageValidator : LocalizedValidator<SendContactMessageCommand>
{
    /// <summary>Longest accepted value for the short single-line fields, in characters.</summary>
    private const int ShortFieldMaxLength = 200;

    /// <summary>Longest accepted message body, in characters.</summary>
    private const int MessageMaxLength = 5000;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SendContactMessageValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(L["ContactFieldRequired", L["ContactFieldName"]])
            .MaximumLength(ShortFieldMaxLength)
            .WithMessage(L["ContactFieldTooLong", L["ContactFieldName"], ShortFieldMaxLength]);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(L["ContactFieldRequired", L["ContactFieldEmail"]])
            .MaximumLength(ShortFieldMaxLength)
            .WithMessage(L["ContactFieldTooLong", L["ContactFieldEmail"], ShortFieldMaxLength])
            .EmailAddress()
            .WithMessage(L["ContactEmailInvalid"]);

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage(L["ContactFieldRequired", L["ContactFieldMessage"]])
            .MaximumLength(MessageMaxLength)
            .WithMessage(L["ContactFieldTooLong", L["ContactFieldMessage"], MessageMaxLength]);

        RuleFor(x => x.Phone)
            .MaximumLength(ShortFieldMaxLength)
            .WithMessage(L["ContactFieldTooLong", L["ContactFieldPhone"], ShortFieldMaxLength])
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Service)
            .MaximumLength(ShortFieldMaxLength)
            .WithMessage(L["ContactFieldTooLong", L["ContactFieldService"], ShortFieldMaxLength])
            .When(x => !string.IsNullOrWhiteSpace(x.Service));

        RuleFor(x => x.SubmittedAt)
            .MaximumLength(ShortFieldMaxLength)
            .WithMessage(L["ContactFieldTooLong", L["ContactFieldSubmittedAt"], ShortFieldMaxLength])
            .When(x => !string.IsNullOrWhiteSpace(x.SubmittedAt));
    }
}
