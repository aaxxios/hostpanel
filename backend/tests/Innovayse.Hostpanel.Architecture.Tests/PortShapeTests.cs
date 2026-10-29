using System.Reflection;
using System.Runtime.CompilerServices;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// A port method takes one typed input record and returns one typed result record once it needs more
/// than one or two values (rules/architecture.md): no tuples, no free string-to-string bags and no
/// long primitive parameter lists on a port.
/// </summary>
public sealed class PortShapeTests
{
    /// <summary>The most primitive parameters a port method takes before the values belong on a record.</summary>
    private const int MaxPrimitiveParameters = 2;

    /// <summary>The value types a parameter list counts as primitive, besides the CLR primitives and string.</summary>
    private static readonly Type[] PrimitiveLike = [typeof(decimal), typeof(Guid), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan)];

    /// <summary>No port method takes or returns a value tuple, anywhere inside its types.</summary>
    [Fact]
    public void PortMethods_UseNoTuples()
    {
        var offenders = PortMethods()
            .Where(method => SignatureOf(method).SelectMany(TypeSignatures.Unwrap).Any(typeof(ITuple).IsAssignableFrom))
            .Select(Describe)
            .ToList();

        offenders.Should().BeEmpty("a port answers with a named record, never a tuple");
    }

    /// <summary>No port method takes or returns a free string-to-string dictionary.</summary>
    [Fact]
    public void PortMethods_UseNoStringBags()
    {
        var offenders = PortMethods()
            .Where(method => SignatureOf(method).SelectMany(TypeSignatures.Unwrap).Any(IsStringBag))
            .Select(Describe)
            .ToList();

        offenders.Should().BeEmpty("a port names every field on a record instead of a string-to-string bag");
    }

    /// <summary>No port method takes more than two primitive parameters; the cancellation token does not count.</summary>
    [Fact]
    public void PortMethods_TakeAtMostTwoPrimitives()
    {
        var offenders = PortMethods()
            .Where(method => method.GetParameters().Count(parameter => IsPrimitive(parameter.ParameterType)) > MaxPrimitiveParameters)
            .Select(Describe)
            .ToList();

        offenders.Should().BeEmpty("more than two values travel to a port as one typed input record");
    }

    /// <summary>The primitive count sees through nullable wrappers, so <c>int?</c> counts like <c>int</c>.</summary>
    [Fact]
    public void PrimitiveCount_SeesThroughNullable()
    {
        IsPrimitive(typeof(int?)).Should().BeTrue();
        IsPrimitive(typeof(string)).Should().BeTrue();
        IsPrimitive(typeof(CancellationToken)).Should().BeFalse();
    }

    /// <summary>Every method declared on a port.</summary>
    /// <returns>The methods, property accessors excluded.</returns>
    private static IEnumerable<MethodInfo> PortMethods()
    {
        return PortPlacementTests.Ports()
            .SelectMany(port => port.GetMethods().Where(method => !method.IsSpecialName));
    }

    /// <summary>The parameter and return types of a method.</summary>
    /// <param name="method">The method.</param>
    /// <returns>Its signature types.</returns>
    private static IEnumerable<Type> SignatureOf(MethodInfo method)
    {
        return method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType);
    }

    /// <summary>Whether a type is a dictionary from string to string, under any of its interfaces.</summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> for a string-to-string bag.</returns>
    private static bool IsStringBag(Type type)
    {
        return type.IsGenericType
            && type.GetGenericArguments() is [var key, var value]
            && key == typeof(string) && value == typeof(string)
            && type.Name.Contains("Dictionary", StringComparison.Ordinal);
    }

    /// <summary>Whether a parameter type counts as a primitive value.</summary>
    /// <param name="type">The parameter type.</param>
    /// <returns><c>true</c> for a CLR primitive, string or one of <see cref="PrimitiveLike"/>, nullable or not.</returns>
    private static bool IsPrimitive(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        return underlying.IsPrimitive || underlying == typeof(string) || PrimitiveLike.Contains(underlying);
    }

    /// <summary>A port method as <c>IPort.Method</c>.</summary>
    /// <param name="method">The method.</param>
    /// <returns>Its readable name.</returns>
    private static string Describe(MethodInfo method)
    {
        return $"{method.DeclaringType!.Name}.{method.Name}";
    }
}
