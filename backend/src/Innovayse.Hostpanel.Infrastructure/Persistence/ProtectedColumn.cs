namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>One column the model stores encrypted.</summary>
/// <param name="Entity">The entity type that owns the property.</param>
/// <param name="Property">The name of the string property.</param>
/// <param name="MaxPlainLength">The longest plain value the property accepts, in characters.</param>
public sealed record ProtectedColumn(Type Entity, string Property, int MaxPlainLength);
