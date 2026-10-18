using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateNetworkIssue;

/// <summary>Validator for <see cref="UpdateNetworkIssueCommand"/>.</summary>
/// <remarks>
/// The handler reads <c>Type</c>, <c>Priority</c> and <c>Status</c> with <c>Enum.Parse</c>;
/// without this, a bad value reached the caller as a 500 rather than a 400.
/// </remarks>
public sealed class UpdateNetworkIssueValidator : LocalizedValidator<UpdateNetworkIssueCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateNetworkIssueValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotNull().GreaterThan(0).WithMessage(L["NetworkIssueIdInvalid"]);
        RuleFor(x => x.Title).NotEmpty().WithMessage(L["NetworkIssueTitleRequired"]);
        RuleFor(x => x.Description).NotEmpty().WithMessage(L["NetworkIssueDescriptionRequired"]);

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<NetworkIssueType>(t, ignoreCase: true, out _))
            .WithMessage(L["NetworkIssueTypeInvalid"]);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<NetworkIssuePriority>(p, ignoreCase: true, out _))
            .WithMessage(L["NetworkIssuePriorityInvalid"]);

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<NetworkIssueStatus>(s, ignoreCase: true, out _))
            .WithMessage(L["NetworkIssueStatusInvalid"]);
    }
}
