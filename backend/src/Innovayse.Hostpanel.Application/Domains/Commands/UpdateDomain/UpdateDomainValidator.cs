using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateDomain;

/// <summary>Validates <see cref="UpdateDomainCommand"/> before the handler executes.</summary>
public sealed class UpdateDomainValidator : LocalizedValidator<UpdateDomainCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateDomainValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);

        RuleFor(x => x.Status)
            .NotNull()
            .IsInEnum()
            .WithMessage(L["DomainStatusInvalid"]);
    }
}
