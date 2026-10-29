namespace Innovayse.Hostpanel.Application.Clients.Common.Enums;

/// <summary>How a client portal invitation grants permissions.</summary>
public enum InvitePermissionMode
{
    /// <summary>The invitee receives every permission; the permission list is empty.</summary>
    All = 0,

    /// <summary>The invitee receives only the permissions listed.</summary>
    Choose = 1,
}
