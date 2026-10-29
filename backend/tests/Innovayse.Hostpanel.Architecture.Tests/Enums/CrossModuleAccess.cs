namespace Innovayse.Hostpanel.Architecture.Tests.Enums;

/// <summary>
/// How a documented exception to module table ownership reaches another module's tables.
/// </summary>
public enum CrossModuleAccess
{
    /// <summary>A read model that only reads tables of several modules.</summary>
    ReadModel = 0,

    /// <summary>A writer that writes tables of several modules, such as a bulk import of legacy data.</summary>
    Writer = 1,
}
