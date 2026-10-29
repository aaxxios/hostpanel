using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Tests.SharedKernel;

/// <summary>Minimal entity subclass for testing <see cref="BaseEntity"/> behaviour.</summary>
internal sealed class ProbeEntity : BaseEntity
{
    /// <summary>
    /// Sets <see cref="BaseEntity.Id"/> through the protected setter, for the test's arrangement.
    /// </summary>
    /// <param name="id">The id to assign.</param>
    public ProbeEntity(int id)
    {
        Id = id;
    }

    /// <summary>Triggers a domain event.</summary>
    public void DoSomething()
    {
        AddDomainEvent(new TestDomainEvent(Id));
    }
}
