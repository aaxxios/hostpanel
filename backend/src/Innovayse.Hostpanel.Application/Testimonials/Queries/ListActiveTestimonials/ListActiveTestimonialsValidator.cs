using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Testimonials.Queries.ListActiveTestimonials;

/// <summary>Validates <see cref="ListActiveTestimonialsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListActiveTestimonialsValidator : LocalizedValidator<ListActiveTestimonialsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListActiveTestimonialsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
