namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Every use-case folder under the Application layer's <c>Commands</c>/<c>Queries</c> directories has
/// a matching <c>&lt;UseCase&gt;Tests.cs</c> under the same feature path in Integration.Tests, so a use
/// case cannot ship without a test that exercises it end to end.
/// </summary>
public sealed class UseCaseShapeTests
{
    /// <summary>
    /// Every <c>Application/&lt;Feature&gt;/{Commands,Queries}/&lt;UseCase&gt;</c> folder has a
    /// sibling <c>Integration.Tests/&lt;Feature&gt;/&lt;UseCase&gt;Tests.cs</c>. Crons and event
    /// handlers under <c>Commands</c> count as use cases too (ruling 4 of the A7 plan); their folder
    /// is held to the same rule.
    /// </summary>
    [Fact]
    public void EveryUseCase_HasAnIntegrationTestFile()
    {
        var backendRoot = FindBackendRoot(AppContext.BaseDirectory);
        var applicationRoot = Path.Combine(
            backendRoot,
            "src",
            "Innovayse.Hostpanel.Application");
        var integrationTestsRoot = Path.Combine(
            backendRoot,
            "tests",
            "Innovayse.Hostpanel.Integration.Tests");

        var missing = new List<string>();

        foreach (var useCaseDirectory in UseCaseDirectories(applicationRoot))
        {
            var featurePath = FeaturePathOf(applicationRoot, useCaseDirectory);
            var useCaseName = Path.GetFileName(useCaseDirectory);
            var expectedTestFile = Path.Combine(
                integrationTestsRoot,
                featurePath,
                $"{useCaseName}Tests.cs");

            if (!File.Exists(expectedTestFile))
            {
                missing.Add(Path.Combine(featurePath, $"{useCaseName}Tests.cs").Replace('\\', '/'));
            }
        }

        missing.Should().BeEmpty(
            "every use case needs Integration.Tests/<Feature>/<UseCase>Tests.cs, but these are missing: {0}",
            string.Join(", ", missing));
    }

    /// <summary>
    /// Walks up from a build output directory to the folder that holds the solution file, so the
    /// test finds the repository layout regardless of which configuration produced the test binary.
    /// </summary>
    /// <param name="startDirectory">The directory to start the upward walk from.</param>
    /// <returns>The directory containing <c>Innovayse.Hostpanel.sln</c>.</returns>
    internal static string FindBackendRoot(string startDirectory)
    {
        var current = new DirectoryInfo(startDirectory);

        while (current is not null)
        {
            if (current.GetFiles("*.sln").Length > 0)
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            $"No solution file found walking up from '{startDirectory}'.");
    }

    /// <summary>
    /// Every leaf directory directly under a <c>Commands</c> or <c>Queries</c> directory within the
    /// Application layer; each such leaf is a use case.
    /// </summary>
    /// <param name="applicationRoot">The Application project's root directory.</param>
    /// <returns>The use-case directories, in no particular order.</returns>
    private static IEnumerable<string> UseCaseDirectories(string applicationRoot)
    {
        foreach (var directory in Directory.EnumerateDirectories(
            applicationRoot,
            "*",
            SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(directory);

            if (name is "Commands" or "Queries")
            {
                foreach (var useCaseDirectory in Directory.EnumerateDirectories(directory))
                {
                    yield return useCaseDirectory;
                }
            }
        }
    }

    /// <summary>
    /// The feature path between the Application root and a use case's <c>Commands</c>/<c>Queries</c>
    /// parent, e.g. <c>Admin/Servers</c> for
    /// <c>Application/Admin/Servers/Commands/CreateServer</c>.
    /// </summary>
    /// <param name="applicationRoot">The Application project's root directory.</param>
    /// <param name="useCaseDirectory">The use-case directory.</param>
    /// <returns>The relative feature path, using the platform's directory separator.</returns>
    private static string FeaturePathOf(string applicationRoot, string useCaseDirectory)
    {
        var commandsOrQueriesDirectory = Path.GetDirectoryName(useCaseDirectory)!;
        var featureDirectory = Path.GetDirectoryName(commandsOrQueriesDirectory)!;

        return Path.GetRelativePath(applicationRoot, featureDirectory);
    }
}
