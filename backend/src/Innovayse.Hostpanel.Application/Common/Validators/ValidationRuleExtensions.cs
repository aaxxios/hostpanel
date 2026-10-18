using FluentValidation;

namespace Innovayse.Hostpanel.Application.Common.Validators;

/// <summary>
/// Rule chains that many validators repeat, named once. A validator states <em>what</em> a property
/// must be (<c>MustBePositiveId</c>, <c>MustBePage</c>) and the limits behind the name live here.
/// </summary>
public static class ValidationRuleExtensions
{
    /// <summary>Requires an identifier to be a positive integer.</summary>
    /// <typeparam name="T">The command or query being validated.</typeparam>
    /// <param name="rule">The rule builder for the id property.</param>
    /// <param name="message">The localized sentence reported when the id is not positive.</param>
    /// <returns>The rule builder, for chaining.</returns>
    public static IRuleBuilderOptions<T, int> MustBePositiveId<T>(this IRuleBuilder<T, int> rule, string message)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.GreaterThan(0).WithMessage(message);
    }

    /// <summary>Requires a 1-based page number.</summary>
    /// <typeparam name="T">The command or query being validated.</typeparam>
    /// <param name="rule">The rule builder for the page property.</param>
    /// <param name="message">The localized sentence reported when the page is below 1.</param>
    /// <returns>The rule builder, for chaining.</returns>
    public static IRuleBuilderOptions<T, int> MustBePage<T>(this IRuleBuilder<T, int> rule, string message)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.GreaterThanOrEqualTo(1).WithMessage(message);
    }

    /// <summary>Requires a page size between 1 and the largest the use case allows.</summary>
    /// <typeparam name="T">The command or query being validated.</typeparam>
    /// <param name="rule">The rule builder for the page-size property.</param>
    /// <param name="maxPageSize">The largest page size this use case accepts.</param>
    /// <param name="message">The localized sentence reported when the size is out of range.</param>
    /// <returns>The rule builder, for chaining.</returns>
    public static IRuleBuilderOptions<T, int> MustBePageSize<T>(
        this IRuleBuilder<T, int> rule, int maxPageSize, string message)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.InclusiveBetween(1, maxPageSize).WithMessage(message);
    }
}
