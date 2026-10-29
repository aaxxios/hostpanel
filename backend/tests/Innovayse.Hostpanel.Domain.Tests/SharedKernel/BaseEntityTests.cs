using Innovayse.Hostpanel.SharedKernel.Domain;

namespace Innovayse.Hostpanel.Domain.Tests.SharedKernel;

/// <summary>Tests for <see cref="BaseEntity"/>: identity equality and the domain-event list.</summary>
public sealed class BaseEntityTests
{
    /// <summary>Aggregate raises domain event added via AddDomainEvent.</summary>
    [Fact]
    public void AddDomainEvent_ShouldAppendEventToList()
    {
        // Arrange
        var probe = new ProbeEntity(1);

        // Act
        probe.DoSomething();

        // Assert
        probe.DomainEvents.Should().ContainSingle();
        probe.DomainEvents.Single().Should().BeOfType<TestDomainEvent>();
    }

    /// <summary>ClearDomainEvents empties the list.</summary>
    [Fact]
    public void ClearDomainEvents_ShouldEmptyTheList()
    {
        // Arrange
        var probe = new ProbeEntity(1);
        probe.DoSomething();

        // Act
        probe.ClearDomainEvents();

        // Assert
        probe.DomainEvents.Should().BeEmpty();
    }

    /// <summary>Two entities of one type with the same non-zero id are equal; different types never are.</summary>
    [Fact]
    public void Equals_IsByTypeAndId()
    {
        var a = new ProbeEntity(7);
        var b = new ProbeEntity(7);
        var c = new OtherProbeEntity(7);

        a.Should().Be(b);
        a.Should().NotBe(c);
        (a == b).Should().BeTrue();
    }

    /// <summary>
    /// Unsaved entities (id 0) are equal only to themselves, so a list of new contacts must not collapse to one.
    /// </summary>
    [Fact]
    public void Equals_UnsavedEntitiesAreDistinct()
    {
        var a = new ProbeEntity(0);
        var b = new ProbeEntity(0);

        a.Should().NotBe(b);
        a.Should().Be(a);
    }
}
