using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Servers;

/// <summary>
/// Represents a named group of provisioning servers with a shared fill strategy.
/// </summary>
public sealed class ServerGroup : BaseEntity, IAggregateRoot
{
    /// <summary>Gets the display name of this group.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the strategy used to assign new accounts across servers in this group.</summary>
    public ServerFillType FillType { get; private set; }

    /// <summary>Gets the UTC timestamp when the group was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Navigation property: servers that belong to this group.</summary>
    public IReadOnlyCollection<Server> Servers => _servers.AsReadOnly();

    /// <summary>Mutable backing list for EF Core.</summary>
    private readonly List<Server> _servers = [];

    /// <summary>EF Core constructor: do not call directly.</summary>
    private ServerGroup() { }

    /// <summary>Initialises a ServerGroup with the given identity.</summary>
    /// <param name="id">Entity identifier.</param>
    private ServerGroup(int id)
    {
        Id = id;
    }

    /// <summary>
    /// Creates a new server group.
    /// </summary>
    /// <param name="name">Display name for the group.</param>
    /// <param name="fillType">Account distribution strategy.</param>
    /// <returns>A new <see cref="ServerGroup"/> instance.</returns>
    public static ServerGroup Create(string name, ServerFillType fillType)
    {
        return new ServerGroup(0)
        {
            Name = name,
            FillType = fillType,
        };
    }

    /// <summary>Renames the group.</summary>
    /// <param name="name">New display name.</param>
    public void Rename(string name)
    {
        Name = name;
    }

    /// <summary>Changes how new accounts are spread over the group's servers.</summary>
    /// <param name="fillType">New fill strategy.</param>
    public void ChangeFillType(ServerFillType fillType)
    {
        FillType = fillType;
    }
}
