using System.Reflection;
using System.Xml.Linq;

namespace EntraNHI.Architecture.Tests;

[TestClass]
public sealed class CoreIndependenceTests
{
    private static readonly IReadOnlySet<string> ForbiddenFrameworkAssemblies =
        new HashSet<string>(
            [
                "System.Net.Http",
                "System.Net.Http.Json"
            ],
            StringComparer.Ordinal);

    [TestMethod]
    public void CoreProjectHasNoProjectReferences()
    {
        string projectPath = GetCoreProjectPath();
        XDocument project = XDocument.Load(projectPath);

        string[] references = project
            .Descendants("ProjectReference")
            .Select(reference =>
                reference.Attribute("Include")?.Value ?? "<missing Include>")
            .ToArray();

        Assert.IsEmpty(
            references,
            $"EntraNHI.Core must not reference another production project. Found: {string.Join(", ", references)}");
    }

    [TestMethod]
    public void CoreProjectHasNoPackageReferences()
    {
        string projectPath = GetCoreProjectPath();
        XDocument project = XDocument.Load(projectPath);

        string[] packages = project
            .Descendants("PackageReference")
            .Select(reference =>
                reference.Attribute("Include")?.Value ?? "<missing Include>")
            .ToArray();

        Assert.IsEmpty(
            packages,
            $"EntraNHI.Core must not contain PackageReference dependencies. Found: {string.Join(", ", packages)}");
    }

    [TestMethod]
    public void CoreAssemblyDoesNotReferenceForbiddenFrameworkAssemblies()
    {
        Assembly coreAssembly = typeof(EntraNHI.Core.AssessmentState).Assembly;

        string[] forbiddenReferences = coreAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name =>
                name is not null &&
                ForbiddenFrameworkAssemblies.Contains(name))
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.IsEmpty(
            forbiddenReferences,
            $"EntraNHI.Core references forbidden framework assemblies: {string.Join(", ", forbiddenReferences)}");
    }

    private static string GetCoreProjectPath()
    {
        string repositoryRoot = FindRepositoryRoot();

        string projectPath = Path.Combine(
            repositoryRoot,
            "src",
            "EntraNHI.Core",
            "EntraNHI.Core.csproj");

        if (!File.Exists(projectPath))
        {
            throw new AssertFailedException(
                $"Core project was not found: {projectPath}");
        }

        return projectPath;
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
