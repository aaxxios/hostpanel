using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Orders.Queries.ListPendingDomainRenewOrderInvoices;

/// <summary>Validates <see cref="ListPendingDomainRenewOrderInvoicesQuery"/>.</summary>
public sealed class ListPendingDomainRenewOrderInvoicesValidator : LocalizedValidator<ListPendingDomainRenewOrderInvoicesQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ListPendingDomainRenewOrderInvoicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ClientIdInvalid"]);
        RuleFor(x => x.DomainName).NotEmpty().WithMessage(L["DomainNameRequired"]);
    }
}
