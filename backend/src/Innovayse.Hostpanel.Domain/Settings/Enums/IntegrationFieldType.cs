namespace Innovayse.Hostpanel.Domain.Settings.Enums;

/// <summary>
/// The input kind of one integration configuration field, as the built-in catalogue declares it
/// and as a plugin manifest names it (matched without regard to case).
/// </summary>
public enum IntegrationFieldType
{
    /// <summary>Single-line plain text.</summary>
    Text,

    /// <summary>A credential typed into a masked box; the built-in catalogue's secret kind.</summary>
    Password,

    /// <summary>A credential; the secret kind a plugin manifest declares.</summary>
    Secret,

    /// <summary>One of a fixed list of options.</summary>
    Select,

    /// <summary>Multi-line text.</summary>
    TextArea,

    /// <summary>An on/off switch stored as <c>true</c> or <c>false</c>.</summary>
    Toggle,
}
