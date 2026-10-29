using Innovayse.Hostpanel.Architecture.Tests.Enums;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// One identifier the model writes into the database.
/// </summary>
/// <param name="Kind">What the identifier names, such as a table or an index.</param>
/// <param name="Name">The identifier itself.</param>
public sealed record DatabaseIdentifier(DatabaseIdentifierKind Kind, string Name);
