using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.TransferOwnership;

/// <summary>Validates <see cref="TransferOwnershipCommand"/> before the handler executes.</summary>
public sealed class TransferOwnershipValidator : LocalizedValidator<TransferOwnershipCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public TransferOwnershipValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.NewOwnerUserId).NotEmpty().WithMessage(L["ClientNewOwnerUserIdRequired"]);
    }
}
