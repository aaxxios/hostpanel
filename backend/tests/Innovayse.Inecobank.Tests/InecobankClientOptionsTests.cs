using Innovayse.Providers.Inecobank;

namespace Innovayse.Inecobank.Tests;

/// <summary>Tests for <see cref="InecobankClientOptions"/>.</summary>
public sealed class InecobankClientOptionsTests
{
    /// <summary><c>ToString</c> omits the password.</summary>
    [Fact]
    public void ToString_OmitsThePassword()
    {
        var options = new InecobankClientOptions("https://testpg.example.am", "merchant-api", "secret-pw");

        var text = options.ToString();

        text.Should().Contain("merchant-api").And.NotContain("secret-pw");
    }
}
