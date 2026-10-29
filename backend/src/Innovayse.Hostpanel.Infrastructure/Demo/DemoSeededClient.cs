using Innovayse.Hostpanel.Domain.Clients;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A demo client and whether the seeding call that returned it created it.</summary>
/// <param name="Client">The client.</param>
/// <param name="Created">Whether this call created the client rather than finding it.</param>
internal sealed record DemoSeededClient(Client Client, bool Created);
