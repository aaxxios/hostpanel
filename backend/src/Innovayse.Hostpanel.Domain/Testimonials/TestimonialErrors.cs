namespace Innovayse.Hostpanel.Domain.Testimonials;

/// <summary>
/// Refusal codes the <see cref="Testimonial"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so the resource coverage test still requires a sentence for each one.
/// </remarks>
public static class TestimonialErrors
{
    /// <summary>The sort order is negative (400): <see cref="Testimonial.Create"/>, <see cref="Testimonial.Update"/> and <see cref="Testimonial.SetSortOrder"/>.</summary>
    public const string SortOrderNegative = "TESTIMONIAL_SORT_ORDER_NEGATIVE";

    /// <summary>The display name is empty or whitespace (400): <see cref="Testimonial.Create"/> and <see cref="Testimonial.Update"/>.</summary>
    public const string NameRequired = "TESTIMONIAL_NAME_REQUIRED";

    /// <summary>The rating is outside 1 to 5 (400): <see cref="Testimonial.Create"/> and <see cref="Testimonial.Update"/>.</summary>
    public const string RatingOutOfRange = "TESTIMONIAL_RATING_OUT_OF_RANGE";
}
