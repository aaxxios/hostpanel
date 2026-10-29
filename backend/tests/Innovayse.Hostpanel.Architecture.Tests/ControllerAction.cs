using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// One HTTP-bound action method and the controller that declares it.
/// </summary>
/// <param name="Controller">The declaring controller type.</param>
/// <param name="Action">The action method.</param>
internal sealed record ControllerAction(Type Controller, MethodInfo Action)
{
    /// <summary>Every HTTP-bound action of every controller in the API assembly.</summary>
    /// <returns>The controller and action method of each one.</returns>
    public static IReadOnlyList<ControllerAction> All()
    {
        return Layers.AuthoredTypesOf(Layers.Api)
            .Where(type => type.IsClass && !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(true).Any())
                .Select(method => new ControllerAction(controller, method)))
            .ToList();
    }
}
