using System.Text.Json.Serialization;

namespace GenMesh.Mesh2Tetra.Tests.TestData;

public sealed class RegressionFixture
{
    public string Name { get; init; } = string.Empty;
    public FixtureInput Input { get; init; } = new();
    public FixtureExpected Expected { get; init; } = new();
    public FixtureOptions Options { get; init; } = new();
}

public sealed class FixtureInput
{
    public required List<double[]> Vertices { get; init; }
    public required List<int[]> Faces { get; init; }
}

public sealed class FixtureExpected
{
    public required int TetraCount { get; init; }
    public required double TetraVolume { get; init; }
    public double VolumeTolerance { get; init; } = 1e-8;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int[]>? ExactTetrahedra { get; init; }
}

public sealed class FixtureOptions
{
    public bool CheckInput { get; init; } = true;
    public bool CheckSelfIntersections { get; init; } = true;
    public bool Verbose { get; init; }
    public bool CheckOutputVolume { get; init; } = true;
    public double PlaneDistanceTolerance { get; init; } = 1e-10;
}
