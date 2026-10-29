using System.Text.RegularExpressions;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Outside Infrastructure and the <c>Providers.*</c> projects no type is named after an external vendor or
/// product: the core speaks in capabilities (payment gateway, registrar), never in whoever supplies one.
/// </summary>
public sealed partial class NoVendorNamesTests
{
    /// <summary>Matches a vendor or external product name anywhere in a type name, case-insensitively.</summary>
    /// <returns>The compiled expression.</returns>
    [GeneratedRegex("Stripe|Cwp|CPanel|Inecobank|Whmcs|Plesk|Namecheap|DirectAdmin|Hestia|PayPal|Cloudflare", RegexOptions.IgnoreCase)]
    private static partial Regex VendorName();

    /// <summary>No type in the kernel, domain, application, shared, API or SDK assemblies carries a vendor name.</summary>
    [Fact]
    public void CoreTypes_CarryNoVendorName()
    {
        var offenders = new[] { Layers.SharedKernel, Layers.Domain, Layers.Application, Layers.Shared, Layers.Api, Layers.Sdk }
            .SelectMany(Layers.AuthoredTypesOf)
            .Where(type => VendorName().IsMatch(type.Name))
            .Select(type => type.FullName)
            .ToList();

        offenders.Should().BeEmpty("name the capability, and keep the vendor in Infrastructure or a Providers project");
    }

    /// <summary>
    /// Matches the text a user can read in C# source: raw, verbatim, regular and interpolated string literals,
    /// character literals (consumed so a quote character does not open a string), and <c>//</c>, <c>///</c> and block comments.
    /// Group <c>text</c> captures string literals and comments; identifiers match nothing.
    /// </summary>
    /// <returns>The compiled expression.</returns>
    [GeneratedRegex(@"'(?:\\.|[^'\\])+'|(?<text>\$*""{3,}[\s\S]*?""{3,})|(?<text>[$@]{1,2}""(?:[^""]|"""")*"")|(?<text>""(?:[^""\\\r\n]|\\.)*"")|(?<text>//[^\r\n]*)|(?<text>/\*[\s\S]*?\*/)")]
    private static partial Regex ReadableText();

    /// <summary>
    /// No demo seed source file names a vendor in a string literal or a comment (regular, verbatim, interpolated and
    /// raw strings; <c>//</c>, <c>///</c> and block comments). Code identifiers are not scanned: vendor identifiers
    /// are legitimate inside Infrastructure, but the seed text a user sees names no outside product.
    /// </summary>
    [Fact]
    public void DemoSeedSources_CarryNoVendorName()
    {
        var backendRoot = UseCaseShapeTests.FindBackendRoot(AppContext.BaseDirectory);
        var demoRoot = Path.Combine(backendRoot, "src", "Innovayse.Hostpanel.Infrastructure", "Demo");

        var offenders = Directory
            .EnumerateFiles(demoRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => ReadableText()
                .Matches(File.ReadAllText(path))
                .Any(match => match.Groups["text"].Success && VendorName().IsMatch(match.Groups["text"].Value)))
            .Select(path => Path.GetRelativePath(backendRoot, path).Replace('\\', '/'))
            .ToList();

        offenders.Should().BeEmpty("demo seed text names no outside product; use the capability name");
    }
}
