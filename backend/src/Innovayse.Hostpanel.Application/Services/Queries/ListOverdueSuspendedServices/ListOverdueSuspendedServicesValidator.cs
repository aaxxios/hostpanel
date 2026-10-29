using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListOverdueSuspendedServices;

/// <summary>Validates <see cref="ListOverdueSuspendedServicesQuery"/>: no rules, the query carries no input.</summary>
public sealed class ListOverdueSuspendedServicesValidator : LocalizedValidator<ListOverdueSuspendedServicesQuery>
{
    /// <summary>Initializes the validator.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ListOverdueSuspendedServicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
