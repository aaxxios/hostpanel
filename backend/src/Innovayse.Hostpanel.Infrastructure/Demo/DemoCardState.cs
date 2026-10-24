using System.Collections.Concurrent;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// What visitors changed on the demo's sample payment methods: which one each customer made the default
/// and which were removed. One instance lives for the whole process (the card gateway is scoped, the
/// changes must outlive a request) and the demo reset clears it together with the database.
/// </summary>
internal sealed class DemoCardState
{
    /// <summary>The default method each customer chose, by customer id.</summary>
    private readonly ConcurrentDictionary<string, string> _chosenDefaults = new();

    /// <summary>The ids of the methods that were removed; every id carries its customer.</summary>
    private readonly ConcurrentDictionary<string, byte> _removed = new();

    /// <summary>Remembers the default a customer chose.</summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="methodId">The method made the default.</param>
    public void ChooseDefault(string customerId, string methodId) => _chosenDefaults[customerId] = methodId;

    /// <summary>Remembers that a method was removed.</summary>
    /// <param name="methodId">The removed method.</param>
    public void Remove(string methodId) => _removed[methodId] = 0;

    /// <summary>Whether a method was removed.</summary>
    /// <param name="methodId">The method.</param>
    /// <returns><see langword="true"/> when it was removed.</returns>
    public bool IsRemoved(string methodId) => _removed.ContainsKey(methodId);

    /// <summary>Reads the default a customer chose.</summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="methodId">The chosen method when there is one.</param>
    /// <returns><see langword="true"/> when the customer chose a default.</returns>
    public bool TryGetDefault(string customerId, out string methodId)
    {
        var found = _chosenDefaults.TryGetValue(customerId, out var chosen);
        methodId = chosen ?? string.Empty;
        return found;
    }

    /// <summary>Forgets every change, so the sample methods read as they did when first seeded.</summary>
    public void Clear()
    {
        _chosenDefaults.Clear();
        _removed.Clear();
    }
}
