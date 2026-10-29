using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateNetworkIssue;

/// <summary>Validator for <see cref="UpdateNetworkIssueCommand"/>.</summary>
/// <remarks>
/// Binding accepts a numeric value that names no member of <c>Type</c>, <c>Priority</c> or
/// <c>Status</c>; the rules below refuse it.
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
            .IsInEnum()
            .WithMessage(L["NetworkIssueTypeInvalid"]);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage(L["NetworkIssuePriorityInvalid"]);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(L["NetworkIssueStatusInvalid"]);
    }
}
