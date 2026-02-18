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
    public List<double[]> Vertices { get; init; } = [];
    public List<int[]> Faces { get; init; } = [];
}

public sealed class FixtureExpected
{
    public int? TetraCount { get; init; }
    public double TetraVolume { get; init; }
    public double VolumeTolerance { get; init; } = 1e-8;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int[]>? ExactTetrahedra { get; init; }

    public string? ExpectedExceptionContains { get; init; }
}

public sealed class FixtureOptions
{
    public bool CheckInput { get; init; } = true;
    public bool? CheckSelfIntersections { get; init; }
    public bool? AutoResolveIntersections { get; init; }
    public bool? FailOnSelfIntersections { get; init; }
    public bool Verbose { get; init; }
    public double PlaneDistanceTolerance { get; init; } = 1e-10;
    public double Epsilon { get; init; } = 1e-8;

    public bool ResolveAutoResolveIntersections()
        => AutoResolveIntersections ?? CheckSelfIntersections ?? true;

    public bool ResolveFailOnSelfIntersections()
        => FailOnSelfIntersections ?? CheckSelfIntersections ?? true;
}
