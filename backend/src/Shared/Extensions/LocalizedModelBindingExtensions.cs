using Innovayse.Hostpanel.Shared.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// MVC model binding that answers in the request's language: no framework-generated English
/// "field is required" messages, and decimals that accept both separators with a localized error.
/// </summary>
public static class LocalizedModelBindingExtensions
{
    /// <summary>
    /// Stops MVC from treating a non-nullable reference type as implicitly required, whose message is
    /// fixed English. Required-ness is the validators' business, where every message is localized.
    /// </summary>
    /// <param name="options">MVC options.</param>
    /// <returns>MVC options for chaining.</returns>
    public static MvcOptions ConfigureLocalizedModelBinding(this MvcOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        return options;
    }

    /// <summary>
    /// Puts <see cref="InvariantDecimalModelBinderProvider"/> first in the binder chain, its message
    /// read from the <typeparamref name="TValidationMessages"/> resource set.
    /// </summary>
    /// <typeparam name="TValidationMessages">
    /// Marker type of the resource set holding <c>InvalidDecimalValue</c>.
    /// </typeparam>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection for chaining.</returns>
    public static IServiceCollection AddInvariantDecimalBinding<TValidationMessages>(this IServiceCollection services)
    {
        services.AddOptions<MvcOptions>()
            .Configure<IStringLocalizerFactory>((options, factory) =>
                options.ModelBinderProviders.Insert(
                    0, new InvariantDecimalModelBinderProvider(factory.Create(typeof(TValidationMessages)))));

        return services;
    }
}
