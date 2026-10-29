using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>
/// A localizer that answers every key with a marked copy of the key, so the test sees exactly
/// which key a rule asked for. A stand-in for the framework's resource localizer, not our code.
/// </summary>
/// <typeparam name="TResource">The resource type the localizer is typed for.</typeparam>
internal sealed class EchoLocalizer<TResource> : IStringLocalizer<TResource>
{
    /// <inheritdoc />
    public LocalizedString this[string name] => new(name, "text:" + name);

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] => this[name];

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return [];
    }
}
