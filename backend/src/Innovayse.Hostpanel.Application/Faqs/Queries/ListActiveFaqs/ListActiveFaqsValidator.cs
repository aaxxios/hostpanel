using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Faqs.Queries.ListActiveFaqs;

/// <summary>Validates <see cref="ListActiveFaqsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListActiveFaqsValidator : LocalizedValidator<ListActiveFaqsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListActiveFaqsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
