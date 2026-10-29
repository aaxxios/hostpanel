using System.Text.RegularExpressions;
using FluentValidation;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.SetupMyService;

/// <summary>Validates <see cref="SetupMyServiceCommand"/> before the handler executes.</summary>
/// <remarks>
/// Mirrors <c>SetupServiceValidator</c>'s field rules exactly, because both commands describe the
/// same hosting details; this one adds the <c>ServiceId</c> shape check the shared command also
/// carries, so a malformed id is refused before ownership is even resolved.
/// </remarks>
public sealed class SetupMyServiceValidator : LocalizedValidator<SetupMyServiceCommand>
{
    /// <summary>
    /// A hostname of two or more dot-separated labels ending in a letters-only TLD, e.g.
    /// <c>example.com</c>. Each label is a to z/0 to 9/hyphen without a leading or trailing hyphen;
    /// the whole name is capped at 253 characters. Case-insensitive.
    /// </summary>
    private static readonly Regex DomainPattern = new(
        @"^(?=.{1,253}$)([a-z0-9](-*[a-z0-9])*\.)+[a-z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SetupMyServiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
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
            .MinimumLength(8).WithMessage(L["ServicePasswordInvalid"])
            .MaximumLength(ProtectedValueLimits.ServicePassword).WithMessage(L["HostingAccountPasswordTooLong"]);
    }
}
