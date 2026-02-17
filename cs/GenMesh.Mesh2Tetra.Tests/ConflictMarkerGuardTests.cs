using Xunit;

namespace GenMesh.Mesh2Tetra.Tests;

public sealed class ConflictMarkerGuardTests
{
    [Fact]
    public void RepositoryFilesMustNotContainMergeConflictMarkers()
    {
        var repoRoot = FindRepositoryRoot();
        var csRoot = Path.Combine(repoRoot, "cs");

        var offenders = Directory
            .EnumerateFiles(csRoot, "*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            .Where(ContainsConflictMarker)
            .Select(path => Path.GetRelativePath(repoRoot, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.True(offenders.Length == 0,
            $"Merge conflict markers were found in: {string.Join(", ", offenders)}");
    }

    private static bool ContainsConflictMarker(string path)
    {
        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("<<<<<<<", StringComparison.Ordinal)
                || line.StartsWith("=======", StringComparison.Ordinal)
                || line.StartsWith(">>>>>>>", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (Directory.Exists(Path.Combine(current, ".git")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}
