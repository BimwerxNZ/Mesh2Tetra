using GenMesh.Mesh2Tetra.Geometry;

namespace GenMesh.Mesh2Tetra.Models;

public readonly record struct Face(int A, int B, int C);

public readonly record struct Tetrahedron(int A, int B, int C, int D)
{
    public int[] Vertices => [A, B, C, D];
}

public sealed record MeshData(IReadOnlyList<Vector3d> Vertices, IReadOnlyList<Face> Faces);
