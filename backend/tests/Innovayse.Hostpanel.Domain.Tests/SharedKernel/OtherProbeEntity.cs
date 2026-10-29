using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Tests.SharedKernel;

/// <summary>A second entity type, distinct from <see cref="ProbeEntity"/> even when the id matches.</summary>
internal sealed class OtherProbeEntity : BaseEntity
{
    /// <summary>
    /// Sets <see cref="BaseEntity.Id"/> through the protected setter, for the test's arrangement.
    /// </summary>
    /// <param name="id">The id to assign.</param>
    public OtherProbeEntity(int id)
    {
        Id = id;
    }
}
