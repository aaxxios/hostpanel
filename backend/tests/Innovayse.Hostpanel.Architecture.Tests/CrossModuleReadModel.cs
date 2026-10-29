using Innovayse.Hostpanel.Architecture.Tests.Enums;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// One documented exception to module table ownership: a handler, or a whole namespace of handlers,
/// that legitimately reaches tables owned by other modules: a read model spanning them, or a
/// documented writer such as the bulk import of legacy data.
/// </summary>
/// <param name="Scope">
/// The full name of the allowed handler type, or a namespace whose handlers are all allowed.
/// </param>
/// <param name="Reason">Why this class has to span modules, in one line.</param>
/// <param name="Access">Whether the exception only reads other modules' tables or also writes them.</param>
internal sealed record CrossModuleReadModel(string Scope, string Reason, CrossModuleAccess Access = CrossModuleAccess.ReadModel)
{
    /// <summary>Whether a handler falls under this exception.</summary>
    /// <param name="handler">The handler type.</param>
    /// <returns><see langword="true"/> when the handler is the scope or sits inside the scope namespace.</returns>
    public bool Covers(Type handler)
    {
        return handler.FullName == Scope
            || (handler.Namespace is not null
                && (handler.Namespace == Scope || handler.Namespace.StartsWith(Scope + ".", StringComparison.Ordinal)));
    }
}
