using System.Text.Json;

namespace GenMesh.Mesh2Tetra.Tests.TestData;

public static class RegressionFixtureLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static IReadOnlyList<object[]> LoadFixtureCases()
    {
        var fixtureDirectory = Path.Combine(AppContext.BaseDirectory, "Fixtures");
        if (!Directory.Exists(fixtureDirectory))
        {
            throw new DirectoryNotFoundException($"Fixture directory not found: {fixtureDirectory}");
        }

        return Directory
            .EnumerateFiles(fixtureDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new object[] { LoadFixture(path), Path.GetFileName(path) })
            .ToArray();
    }

    private static RegressionFixture LoadFixture(string path)
    {
        var content = File.ReadAllText(path);
        var fixture = JsonSerializer.Deserialize<RegressionFixture>(content, JsonOptions)
            ?? throw new InvalidDataException($"Failed to parse fixture file: {path}");

        ValidateFixture(fixture, path);
        return fixture;
    }

    private static void ValidateFixture(RegressionFixture fixture, string path)
    {
        if (fixture.Input.Vertices.Count < 4)
        {
            throw new InvalidDataException($"Fixture '{path}' has fewer than 4 vertices.");
        }

        if (fixture.Input.Faces.Count < 4)
        {
            throw new InvalidDataException($"Fixture '{path}' has fewer than 4 faces.");
        }

        var vertexCount = fixture.Input.Vertices.Count;
        foreach (var face in fixture.Input.Faces)
        {
            if (face.Length != 3)
            {
                throw new InvalidDataException($"Fixture '{path}' has a non-triangular face.");
            }

            foreach (var vertexId in face)
            {
                if (vertexId < 0 || vertexId >= vertexCount)
                {
                    throw new InvalidDataException($"Fixture '{path}' face index is out of range.");
                }
            }
        }
    }
}
