using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Plugins;

/// <summary>
/// Implementation type that deliberately does not implement <see cref="IPaymentPlugin"/>,
/// standing in for a third-party plugin whose manifest mistakenly (or maliciously) claims
/// <see cref="PluginType.Payment"/> for an entry point that isn't one.
/// </summary>
internal sealed class MistypedManifestPlugin
{
}
