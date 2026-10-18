namespace Innovayse.Hostpanel.SharedKernel.Domain;

/// <summary>
/// A value defined entirely by its components: two instances of the same type with equal
/// components are the same value.
/// </summary>
/// <remarks>
/// The concrete type is part of the identity, so a <c>Price</c> and a <c>Cost</c> holding the same
/// amount are never equal. Components are compared in order with <see cref="object.Equals(object, object)"/>,
/// which treats two null components as equal.
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Whether two value objects are equal; two nulls are equal.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> when both are null or both are equal values.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>Whether two value objects differ.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> when exactly one is null or the values differ.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = default(HashCode);
        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    /// <summary>The components that define the value, in a stable order.</summary>
    /// <returns>The components compared by <see cref="Equals(ValueObject)"/>.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
