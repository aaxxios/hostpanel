using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateNetworkIssue;

/// <summary>Validator for <see cref="CreateNetworkIssueCommand"/>.</summary>
/// <remarks>
/// Binding accepts a numeric value that names no member of <c>Type</c> or <c>Priority</c>; the
/// rules below refuse it.
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

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(L["NetworkIssueTypeInvalid"]);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage(L["NetworkIssuePriorityInvalid"]);
    }
}
