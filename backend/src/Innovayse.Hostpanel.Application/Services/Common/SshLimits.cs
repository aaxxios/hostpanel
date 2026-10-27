namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>Fixed limits of SSH key management.</summary>
public static class SshLimits
{
    /// <summary>Most keys one service may hold. The agent enforces its own, higher ceiling too.</summary>
    public const int MaxKeysPerService = 20;

    /// <summary>Longest label accepted.</summary>
    public const int MaxLabelLength = 100;

    /// <summary>Longest pasted public key accepted; a real ED25519 line is under 120 characters.</summary>
    public const int MaxPublicKeyLength = 1024;
}
