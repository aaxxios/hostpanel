using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Testimonials.Queries.ListTestimonials;

/// <summary>Validates <see cref="ListTestimonialsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListTestimonialsValidator : LocalizedValidator<ListTestimonialsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListTestimonialsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
