namespace Innovayse.Hostpanel.Application.Admin.Common.Enums;

/// <summary>What a write did to a secret setting, as recorded in the audit log.</summary>
public enum SecretSettingChange
{
    /// <summary>A new value replaced whatever was stored.</summary>
    Replaced = 1,

    /// <summary>An explicit clear removed the stored value.</summary>
    Cleared = 2,
}
