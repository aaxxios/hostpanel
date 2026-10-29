namespace Innovayse.Hostpanel.Architecture.Tests.Enums;

/// <summary>
/// What a database identifier names.
/// </summary>
public enum DatabaseIdentifierKind
{
    /// <summary>A table name.</summary>
    Table = 0,

    /// <summary>A column name, written as <c>Table.Column</c>.</summary>
    Column = 1,

    /// <summary>A primary, alternate or foreign key name.</summary>
    Key = 2,

    /// <summary>An index name.</summary>
    Index = 3,

    /// <summary>A sequence name.</summary>
    Sequence = 4,

    /// <summary>A foreign key constraint name.</summary>
    ForeignKey = 5,

    /// <summary>A check constraint name.</summary>
    CheckConstraint = 6,
}
