namespace GenMesh.Mesh2Tetra;

public sealed class Mesh2TetraOptions
{
    public bool Verbose { get; init; } = true;
    public bool CheckInput { get; init; } = true;
    public double Epsilon { get; init; } = 1e-8;
}
