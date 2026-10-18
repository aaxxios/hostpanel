using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Queries.ListClients;

/// <summary>
///     Validator for ListClientsQuery. Page and page size are both clamped by the handler, so no
///     rule is declared for them here — decided, not forgotten.
/// </summary>
public sealed class ListClientsValidator : LocalizedValidator<ListClientsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListClientsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
