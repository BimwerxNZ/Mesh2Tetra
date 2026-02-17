namespace GenMesh.Mesh2Tetra;

public sealed class Mesh2TetraOptions
{
    public bool Verbose { get; init; } = true;
    public bool CheckInput { get; init; } = true;
    public bool AutoFixFaceOrientation { get; init; } = true;
    public bool AutoResolveIntersections { get; init; } = true;
    public bool FailOnSelfIntersections { get; init; } = true;
    public double Epsilon { get; init; } = 1e-8;
    public double PlaneDistanceTolerance { get; init; } = 1e-10;
    public int MaxDelaunayRecursionDepth { get; init; } = 8;
}
