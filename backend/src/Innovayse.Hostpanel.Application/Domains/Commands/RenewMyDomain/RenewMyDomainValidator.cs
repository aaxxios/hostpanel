using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.RenewMyDomain;

/// <summary>Validates <see cref="RenewMyDomainCommand"/> inputs before the handler executes.</summary>
/// <remarks>
/// The same shape as <c>TransferMyDomainValidator</c>, for the same reasons. The year bound is
/// stated here as well as in the TLD price table: a period the table has no entry for is refused
/// later anyway, but a caller who sends 0 or 500 should be told so before an order number is
/// burned on it. There is no rule about who owns the domain: that is
/// <c>IDomainOwnership</c>'s, in the handler, because it needs the credential.
/// </remarks>
public sealed class RenewMyDomainValidator : LocalizedValidator<RenewMyDomainCommand>
{
    /// <summary>Initializes validation rules for a client-initiated domain renewal.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RenewMyDomainValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
        RuleFor(x => x.Years).InclusiveBetween(1, 10).WithMessage(L["DomainYearsRange"]);
        RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage(L["PaymentMethodRequired"]);
    }
}
