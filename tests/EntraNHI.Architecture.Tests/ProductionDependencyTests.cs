using System.Xml.Linq;

namespace EntraNHI.Architecture.Tests;

[TestClass]
public sealed class ProductionDependencyTests
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>>
        ExpectedDependencies =
            new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
            {
                ["EntraNHI.Core"] =
                    new HashSet<string>(StringComparer.Ordinal),

                ["EntraNHI.Application"] =
                    new HashSet<string>(
                        ["EntraNHI.Core"],
                        StringComparer.Ordinal),

                ["EntraNHI.Infrastructure.Graph"] =
                    new HashSet<string>(
                        [
                            "EntraNHI.Core",
                            "EntraNHI.Application"
                        ],
                        StringComparer.Ordinal),

                ["EntraNHI.Output"] =
                    new HashSet<string>(
                        [
                            "EntraNHI.Core",
                            "EntraNHI.Application"
                        ],
                        StringComparer.Ordinal),

                ["EntraNHI.Cli"] =
                    new HashSet<string>(
                        [
                            "EntraNHI.Core",
                            "EntraNHI.Application",
                            "EntraNHI.Infrastructure.Graph",
                            "EntraNHI.Output"
                        ],
                        StringComparer.Ordinal)
            };

    [TestMethod]
    public void ProductionProjectReferencesMatchApprovedDag()
    {
        string repositoryRoot = FindRepositoryRoot();

        var actualDependencies = Directory
            .EnumerateFiles(
                Path.Combine(repositoryRoot, "src"),
                "*.csproj",
                SearchOption.AllDirectories)
            .ToDictionary(
                projectPath => Path.GetFileNameWithoutExtension(projectPath),
                projectPath => ReadProductionProjectReferences(
                    repositoryRoot,
                    projectPath),
                StringComparer.Ordinal);

        CollectionAssert.AreEquivalent(
            ExpectedDependencies.Keys.ToArray(),
            actualDependencies.Keys.ToArray(),
            "Production project set differs from the approved architecture.");

        foreach (var expected in ExpectedDependencies)
        {
            CollectionAssert.AreEquivalent(
                expected.Value.ToArray(),
                actualDependencies[expected.Key].ToArray(),
                $"Project references for {expected.Key} differ from the approved DAG.");
        }
    }

    private static IReadOnlySet<string> ReadProductionProjectReferences(
        string repositoryRoot,
        string projectPath)
    {
        XDocument project = XDocument.Load(projectPath);

        return project
            .Descendants("ProjectReference")
            .Select(reference =>
                reference.Attribute("Include")?.Value
                ?? throw new AssertFailedException(
                    $"ProjectReference without Include in {projectPath}."))
            .Select(reference =>
                Path.GetFullPath(
                    Path.Combine(
                        Path.GetDirectoryName(projectPath)
                            ?? throw new AssertFailedException(
                                $"Cannot determine project directory: {projectPath}"),
                        reference)))
            .Select(referencePath =>
            {
                string srcRoot = Path.GetFullPath(
                    Path.Combine(repositoryRoot, "src"))
                    + Path.DirectorySeparatorChar;

                if (!referencePath.StartsWith(
                        srcRoot,
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
