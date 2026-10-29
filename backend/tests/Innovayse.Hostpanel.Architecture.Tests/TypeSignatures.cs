using System.Reflection;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>Reads the types a type's member signatures name, for rules about what a layer may see.</summary>
internal static class TypeSignatures
{
    /// <summary>Every member binding a signature rule reads: instance and static, any visibility.</summary>
    public const BindingFlags AllDeclared = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
        | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    /// <summary>
    /// The types named in a type's signatures: field and property types, constructor and method
    /// parameter types and method return types, generic arguments unwrapped.
    /// </summary>
    /// <param name="type">The type to read.</param>
    /// <returns>Every type its members mention.</returns>
    public static IEnumerable<Type> TypesNamedBy(Type type)
    {
        var fields = type.GetFields(AllDeclared).Select(field => field.FieldType);
        var properties = type.GetProperties(AllDeclared).Select(property => property.PropertyType);
        var constructors = type.GetConstructors(AllDeclared).SelectMany(ctor => ctor.GetParameters()).Select(parameter => parameter.ParameterType);
        var methods = type.GetMethods(AllDeclared)
            .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType));

        return fields.Concat(properties).Concat(constructors).Concat(methods).SelectMany(Unwrap);
    }

    /// <summary>A type and every generic argument, element type and by-ref target inside it, recursively.</summary>
    /// <param name="type">The type.</param>
    /// <returns>The type and its constituents.</returns>
    public static IEnumerable<Type> Unwrap(Type type)
    {
        yield return type;

        Type[] inner = type.HasElementType ? [type.GetElementType()!] : type.IsGenericType ? type.GetGenericArguments() : [];
        foreach (var constituent in inner.SelectMany(Unwrap))
        {
            yield return constituent;
        }
    }
}
