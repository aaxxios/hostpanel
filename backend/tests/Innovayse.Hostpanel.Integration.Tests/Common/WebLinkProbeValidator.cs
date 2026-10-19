using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;

namespace Innovayse.Hostpanel.Integration.Tests.Common;

/// <summary>Applies the shared web-link rule once, so the rule can be tested on its own.</summary>
internal sealed class WebLinkProbeValidator : AbstractValidator<WebLinkProbe>
{
    /// <summary>Message the probe reports for a refused link.</summary>
    public const string Message = "link";

    /// <summary>Initializes the validator and declares its rule.</summary>
    public WebLinkProbeValidator()
    {
        RuleFor(x => x.Link).MustBeWebLink(Message);
    }
}
