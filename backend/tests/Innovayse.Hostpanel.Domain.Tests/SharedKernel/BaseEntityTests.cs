using FluentAssertions;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.SharedKernel;
/// <summary>Tests for <see cref="BaseEntity"/> — identity equality and the domain-event list.</summary>
public sealed class BaseEntityTests
{
    /// <summary>Aggregate raises domain event added via AddDomainEvent.</summary>
    [Fact]
    public void AddDomainEvent_ShouldAppendEventToList()
    {
        // Arrange
        var probe = new Probe(1);

        // Act
        probe.DoSomething();

        // Assert
        probe.DomainEvents.Should().ContainSingle();
        probe.DomainEvents.Single().Should().BeOfType<TestEvent>();
    }

    /// <summary>ClearDomainEvents empties the list.</summary>
    [Fact]
    public void ClearDomainEvents_ShouldEmptyTheList()
    {
        // Arrange
        var probe = new Probe(1);
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
        var a = new Probe(7);
        var b = new Probe(7);
        var c = new OtherProbe(7);

        a.Should().Be(b);
        a.Should().NotBe(c);
        (a == b).Should().BeTrue();
    }

    /// <summary>
    /// Unsaved entities (id 0) are equal only to themselves — a list of new contacts must not collapse to one.
    /// </summary>
    [Fact]
    public void Equals_UnsavedEntitiesAreDistinct()
    {
        var a = new Probe(0);
        var b = new Probe(0);

        a.Should().NotBe(b);
        a.Should().Be(a);
    }

    /// <summary>Minimal entity subclass for testing <see cref="BaseEntity"/> behaviour.</summary>
    private sealed class Probe : BaseEntity
    {
        /// <summary>
        /// Sets <see cref="BaseEntity.Id"/> through the protected setter, for the test's arrangement.
        /// </summary>
        /// <param name="id">The id to assign.</param>
        public Probe(int id)
        {
            Id = id;
        }

        /// <summary>Triggers a domain event.</summary>
        public void DoSomething()
        {
            AddDomainEvent(new TestEvent(Id));
        }
    }

    /// <summary>A second entity type, distinct from <see cref="Probe"/> even when the id matches.</summary>
    private sealed class OtherProbe : BaseEntity
    {
        /// <summary>
        /// Sets <see cref="BaseEntity.Id"/> through the protected setter, for the test's arrangement.
        /// </summary>
        /// <param name="id">The id to assign.</param>
        public OtherProbe(int id)
        {
            Id = id;
        }
    }

    /// <summary>Test domain event.</summary>
    /// <param name="EntityId">The id of the entity that raised the event.</param>
    private sealed record TestEvent(int EntityId) : IDomainEvent
    {
        /// <inheritdoc />
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
