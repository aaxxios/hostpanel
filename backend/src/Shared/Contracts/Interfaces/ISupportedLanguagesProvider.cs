using System.Diagnostics.CodeAnalysis;

namespace Innovayse.Hostpanel.Shared.Contracts.Interfaces;

/// <summary>The languages the platform serves, as a replaceable service over the static list.</summary>
public interface ISupportedLanguagesProvider
{
    /// <summary>Every supported ISO 639-1 code.</summary>
    public IReadOnlyList<string> Languages { get; }

    /// <summary>The language used when a request asks for nothing supported.</summary>
    [SuppressMessage(
        "Naming",
        "CA1716:Identifiers should not match keywords",
        Justification = "House API member name; this codebase is C#-only.")]
    public string Default { get; }

    /// <summary>Whether <paramref name="language"/> is served.</summary>
    /// <param name="language">A lower-case ISO 639-1 code.</param>
    /// <returns><c>true</c> when supported.</returns>
    public bool IsSupported(string language);
}
