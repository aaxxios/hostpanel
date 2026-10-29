using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Domain.Auth.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Auth;

/// <summary>Unit tests for <see cref="SubjectRole"/> and the <see cref="Roles"/> names.</summary>
public sealed class SubjectRoleTests
{
    /// <summary>A grant carries the subject and the role it was given.</summary>
    [Fact]
    public void Grant_CarriesSubjectAndRole()
    {
        var grant = SubjectRole.Grant("sub-1", Role.Reseller);

        grant.Subject.Should().Be("sub-1");
        grant.Role.Should().Be(Role.Reseller);
    }

    /// <summary>A grant without a subject is refused, so no row can authorize nobody.</summary>
    /// <param name="subject">A blank subject.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Grant_RefusesABlankSubject(string subject)
    {
        var act = () => SubjectRole.Grant(subject, Role.Admin);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// The role names are the enum member names. The database rows and the role claims hold these
    /// strings, so a rename of either side would lock existing holders out.
    /// </summary>
    [Fact]
    public void Roles_MatchTheStoredNames()
    {
        Roles.Admin.Should().Be("Admin").And.Be(Role.Admin.ToString());
        Roles.Reseller.Should().Be("Reseller").And.Be(Role.Reseller.ToString());
        Roles.Client.Should().Be("Client").And.Be(Role.Client.ToString());
    }
}
