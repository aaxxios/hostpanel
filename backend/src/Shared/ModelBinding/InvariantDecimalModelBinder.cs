using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Shared.ModelBinding;

/// <summary>
///     Model binder for <see cref="decimal"/> and <see cref="Nullable{Decimal}"/> that accepts both
///     period and comma as decimal separators, regardless of the current request culture.
///     Fixes form submissions where localized cultures (hy/ru) use comma but users type period.
/// </summary>
public sealed class InvariantDecimalModelBinder : IModelBinder
{
    /// <summary>
    ///     <see cref="NumberStyles.Number"/> without <see cref="NumberStyles.AllowThousands"/>.
    ///     Adaptation: with thousands allowed the invariant parse reads <c>"3,1"</c> as 31 (group
    ///     separators are not position-checked), so the comma fallback below would never be reached.
    /// </summary>
    private const NumberStyles Styles = NumberStyles.Number & ~NumberStyles.AllowThousands;

    /// <summary>Whether the target type is <see cref="Nullable{Decimal}"/>.</summary>
    private readonly bool _isNullable;

    /// <summary>Localizer used to resolve the error message per request culture.</summary>
    private readonly IStringLocalizer _localizer;

    /// <summary>
    ///     Initializes a new instance with the given nullability and localizer.
    /// </summary>
    /// <param name="isNullable">Whether the target type is <see cref="Nullable{Decimal}"/>.</param>
    /// <param name="localizer">String localizer for resolving the error message at request time.</param>
    public InvariantDecimalModelBinder(bool isNullable, IStringLocalizer localizer)
    {
        _isNullable = isNullable;
        _localizer = localizer;
    }

    /// <inheritdoc/>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrWhiteSpace(value))
        {
            bindingContext.Result = _isNullable
                ? ModelBindingResult.Success(null)
                : ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Try InvariantCulture first (period separator: "3.1")
        // then fall back to current culture (comma separator: "3,1")
        if (decimal.TryParse(value, Styles, CultureInfo.InvariantCulture, out var result) ||
            decimal.TryParse(value, Styles, CultureInfo.CurrentCulture, out result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            var message = _localizer["InvalidDecimalValue"];
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                string.Format(
                    CultureInfo.CurrentCulture,
                    message.ResourceNotFound ? "The value '{0}' is not a valid number." : message.Value,
                    value));
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}
