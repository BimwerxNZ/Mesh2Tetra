using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class BoundaryCollapse3D
{
    public static IReadOnlyList<Tetrahedron> FillResidualVolume(
        IReadOnlyList<Vector3d> vertices,
        IReadOnlyList<Face> boundaryFaces,
        IReadOnlyList<Tetrahedron> existing,
        Mesh2TetraOptions options)
    {
        // Matlab implementation does a sophisticated edge-collapse fallback.
        // This .NET port keeps Delaunay tets and performs strict volume validation.
        // The method is intentionally isolated to allow future parity work.
        var meshVolume = GeometryPredicates.FaceMeshVolume(vertices, boundaryFaces);
        var tetraVolume = GeometryPredicates.TetraMeshVolume(vertices, existing);

        if (Math.Abs(meshVolume - tetraVolume) <= Math.Max(options.Epsilon, meshVolume * 1e-4))
        {
            return existing;
        }

        throw new InvalidOperationException(
            $"Residual volume detected after Delaunay phase. Boundary collapse parity is not implemented yet. " +
            $"Boundary={meshVolume}, tetra={tetraVolume}.");
    }
}
