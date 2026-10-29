using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>A validator declaring one rule whose message is looked up by key.</summary>
internal sealed class NameProbeValidator : LocalizedValidator<NameProbe>
{
    /// <summary>Declares the rule.</summary>
    /// <param name="localizer">The sentences the rule's message is looked up in.</param>
    public NameProbeValidator(IStringLocalizer localizer)
        : base(localizer)
    {
        RuleFor(p => p.Name).NotEmpty().WithMessage(L["NameRequired"]);
    }
}
