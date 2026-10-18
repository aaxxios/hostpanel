using System.Text.RegularExpressions;
using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.SetupService;
/// <summary>Validates <see cref="SetupServiceCommand"/> before the service is provisioned.</summary>
/// <remarks>
/// The chokepoint every provisioning path funnels through: the client-facing
/// <c>SetupMyServiceCommand</c> delegates here, and any future staff route dispatches this same
/// command, so the rules cannot be bypassed by choosing a different entry. They mirror the
/// setup wizard's own field checks, because a form check the server does not repeat is not a
/// check at all — a caller reaching the endpoint directly must be refused a malformed domain,
/// a bad username, or a too-short password exactly as the UI would refuse them.
/// </remarks>
public sealed class SetupServiceValidator : LocalizedValidator<SetupServiceCommand>
{
    /// <summary>
    /// A hostname of two or more dot-separated labels ending in a letters-only TLD, e.g.
    /// <c>example.com</c>. Each label is a–z/0–9/hyphen without a leading or trailing hyphen;
    /// the whole name is capped at 253 characters. Case-insensitive.
    /// </summary>
    private static readonly Regex DomainPattern = new(
        @"^(?=.{1,253}$)([a-z0-9](-*[a-z0-9])*\.)+[a-z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SetupServiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage(L["ServiceDomainRequired"])
            .Must(d => DomainPattern.IsMatch(d.Trim()))
            .WithMessage(L["ServiceDomainInvalid"]);

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(L["ServiceUsernameRequired"])
            .Matches("^[a-z][a-z0-9]{2,7}$")
            .WithMessage(L["ServiceUsernameInvalid"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(L["ServicePasswordRequired"])
            .MinimumLength(8).WithMessage(L["ServicePasswordInvalid"]);
    }
}
