using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Faqs.Queries.ListFaqs;

/// <summary>Validates <see cref="ListFaqsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListFaqsValidator : LocalizedValidator<ListFaqsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListFaqsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
