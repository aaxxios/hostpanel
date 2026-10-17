using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Support.Commands.CreateNetworkIssue;

/// <summary>Validator for <see cref="CreateNetworkIssueCommand"/>.</summary>
/// <remarks>
/// The handler reads <c>Type</c> and <c>Priority</c> with <c>Enum.Parse</c>; without this, a bad
/// value reached the caller as a 500 rather than a 400.
/// </remarks>
public sealed class CreateNetworkIssueValidator : LocalizedValidator<CreateNetworkIssueCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateNetworkIssueValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["NetworkIssueTitleRequired"]);
        RuleFor(x => x.Description).NotEmpty().WithMessage(L["NetworkIssueDescriptionRequired"]);

        // ignoreCase on both, matching the handler's Enum.Parse<...>(value, ignoreCase: true) exactly.
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<NetworkIssueType>(t, ignoreCase: true, out _))
            .WithMessage(L["NetworkIssueTypeInvalid"]);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<NetworkIssuePriority>(p, ignoreCase: true, out _))
            .WithMessage(L["NetworkIssuePriorityInvalid"]);
    }
}
