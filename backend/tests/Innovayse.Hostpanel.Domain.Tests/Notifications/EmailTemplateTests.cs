using Innovayse.Hostpanel.Domain.Notifications;

namespace Innovayse.Hostpanel.Domain.Tests.Notifications;

/// <summary>Unit tests for the <see cref="EmailTemplate"/> entity.</summary>
public sealed class EmailTemplateTests
{
    /// <summary>Create keeps the language the version is written in, normalised to a lower-case code.</summary>
    [Fact]
    public void Create_KeepsTheLanguage()
    {
        var template = EmailTemplate.Create("welcome", " RU ", "Subject", "Body", null);

        template.Slug.Should().Be("welcome");
        template.Language.Should().Be("ru");
        template.IsActive.Should().BeTrue();
    }

    /// <summary>Create refuses a version with no language, since a recipient could never be matched to it.</summary>
    [Fact]
    public void Create_WithoutLanguage_Throws()
    {
        var create = () => EmailTemplate.Create("welcome", " ", "Subject", "Body", null);

        create.Should().Throw<ArgumentException>();
    }

    /// <summary>Update changes the content and leaves the language alone.</summary>
    [Fact]
    public void Update_KeepsTheLanguage()
    {
        var template = EmailTemplate.Create("welcome", "hy", "Subject", "Body", null);

        template.Update("New subject", "New body", "Description");

        template.Language.Should().Be("hy");
        template.Subject.Should().Be("New subject");
        template.Body.Should().Be("New body");
    }
}
