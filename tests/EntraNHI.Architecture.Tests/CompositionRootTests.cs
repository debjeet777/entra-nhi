using System.Xml.Linq;

namespace EntraNHI.Architecture.Tests;

[TestClass]
public sealed class CompositionRootTests
{
    private const string CliProject = "EntraNHI.Cli";
    private const string GraphProject = "EntraNHI.Infrastructure.Graph";
    private const string OutputProject = "EntraNHI.Output";

    [TestMethod]
    public void CliIsTheOnlyProductionProjectThatCanComposeConcreteOuterComponents()
    {
        string repositoryRoot = FindRepositoryRoot();

        Dictionary<string, IReadOnlySet<string>> dependencies =
            ReadProductionDependencies(repositoryRoot);

        string[] compositionCandidates = dependencies
            .Where(entry =>
                entry.Value.Contains(GraphProject) &&
                entry.Value.Contains(OutputProject))
            .Select(entry => entry.Key)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { CliProject },
            compositionCandidates,
            "Only EntraNHI.Cli may reference both concrete outer components.");
    }

    [TestMethod]
    public void NoProductionProjectReferencesCli()
    {
        string repositoryRoot = FindRepositoryRoot();

        Dictionary<string, IReadOnlySet<string>> dependencies =
            ReadProductionDependencies(repositoryRoot);

        string[] projectsReferencingCli = dependencies
            .Where(entry => entry.Value.Contains(CliProject))
            .Select(entry => entry.Key)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.IsEmpty(
            projectsReferencingCli,
            $"No production project may depend on {CliProject}. Found: {string.Join(", ", projectsReferencingCli)}");
    }

    private static Dictionary<string, IReadOnlySet<string>>
        ReadProductionDependencies(string repositoryRoot)
    {
        string srcRoot = Path.GetFullPath(
            Path.Combine(repositoryRoot, "src"));

        return Directory
            .EnumerateFiles(
                srcRoot,
                "*.csproj",
                SearchOption.AllDirectories)
            .ToDictionary(
                projectPath => Path.GetFileNameWithoutExtension(projectPath),
                projectPath => ReadProjectReferences(
                    srcRoot,
                    projectPath),
                StringComparer.Ordinal);
    }

    private static IReadOnlySet<string> ReadProjectReferences(
        string srcRoot,
        string projectPath)
    {
        XDocument project = XDocument.Load(projectPath);

        string projectDirectory =
            Path.GetDirectoryName(projectPath)
            ?? throw new AssertFailedException(
                $"Cannot determine project directory: {projectPath}");

        string normalizedSrcRoot =
            Path.GetFullPath(srcRoot)
            + Path.DirectorySeparatorChar;

        return project
            .Descendants("ProjectReference")
            .Select(reference =>
                reference.Attribute("Include")?.Value
                ?? throw new AssertFailedException(
                    $"ProjectReference without Include in {projectPath}."))
            .Select(reference =>
                Path.GetFullPath(
                    Path.Combine(projectDirectory, reference)))
            .Select(referencePath =>
            {
                if (!referencePath.StartsWith(
                        normalizedSrcRoot,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new AssertFailedException(
                        $"Production project references a project outside src/: {referencePath}");
                }

                return Path.GetFileNameWithoutExtension(referencePath);
            })
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory =
            new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(
                    Path.Combine(directory.FullName, "EntraNHI.slnx")) &&
                Directory.Exists(
                    Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new AssertFailedException(
            "Unable to locate the EntraNHI repository root.");
    }
}
