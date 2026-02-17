using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class MeshValidation
{
    public static void ValidateInput(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> faces)
    {
        if (vertices.Count < 4)
        {
            throw new ArgumentException("A closed 3D mesh requires at least 4 vertices.");
        }

        if (faces.Count < 4)
        {
            throw new ArgumentException("A closed 3D mesh requires at least 4 faces.");
        }

        foreach (var face in faces)
        {
            if (face.A < 0 || face.B < 0 || face.C < 0 ||
                face.A >= vertices.Count || face.B >= vertices.Count || face.C >= vertices.Count)
            {
                throw new ArgumentException("Face index is out of range.");
            }

            if (face.A == face.B || face.B == face.C || face.A == face.C)
            {
                throw new ArgumentException("Degenerate face found.");
            }
        }
    }
}
