namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// The generated API document is built from the XML docs of the assemblies whose types it shows
/// (rules/api.md): the controllers, the commands, queries and DTOs, and the shared error body. A
/// build that stops emitting one of those files silently drops every description from the document.
/// </summary>
public sealed class ApiDocumentationTests
{
    /// <summary>The assemblies whose XML docs the document generator includes.</summary>
    /// <returns>API, Application and Shared.</returns>
    public static TheoryData<string> DocumentedAssemblies()
    {
        var data = new TheoryData<string>();
        foreach (var assembly in new[] { Layers.Api, Layers.Application, Layers.Shared })
        {
            data.Add(assembly.GetName().Name!);
        }

        return data;
    }

    /// <summary>Each documented assembly ships its XML docs file beside its binary.</summary>
    /// <param name="assemblyName">The assembly name.</param>
    [Theory]
    [MemberData(nameof(DocumentedAssemblies))]
    public void XmlDocsFile_IsEmitted(string assemblyName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");

        File.Exists(path).Should().BeTrue("the document generator reads {0} for its descriptions", path);
    }
}
