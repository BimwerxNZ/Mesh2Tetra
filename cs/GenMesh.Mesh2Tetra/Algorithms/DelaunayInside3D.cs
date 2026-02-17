using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;
using MIConvexHull;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class DelaunayInside3D
{
    public static (IReadOnlyList<Tetrahedron> Tetrahedra, IReadOnlyList<Face> RemainingFaces) Build(
        IReadOnlyList<Vector3d> vertices,
        IReadOnlyList<Face> boundaryFaces,
        Mesh2TetraOptions options)
    {
        var tetrahedra = BuildRecursive(vertices, boundaryFaces, options, depth: 0);
        var remainingFaces = MeshTopology.GetRemainingFaces(tetrahedra, boundaryFaces);
        return (tetrahedra, remainingFaces);
    }

    private static List<Tetrahedron> BuildRecursive(
        IReadOnlyList<Vector3d> vertices,
        IReadOnlyList<Face> faces,
        Mesh2TetraOptions options,
        int depth)
    {
        var objects = MeshTopology.SeparateFaceObjects(faces);
        var total = new List<Tetrahedron>();

        foreach (var obj in objects)
        {
            var (localVertices, localFaces, globalVertexIds) = MeshTopology.InsidePoints3D(vertices, obj);
            if (localVertices.Count < 4 || localFaces.Count < 4)
            {
                continue;
            }

            var localTets = BuildLocal(localVertices, localFaces, options);
            if (localTets.Count == 0)
            {
                continue;
            }

            var localRemaining = MeshTopology.GetRemainingFaces(localTets, localFaces);
            var localBoundaryVolume = GeometryPredicates.FaceMeshVolume(localVertices, localFaces);
            var localTetVolume = GeometryPredicates.TetraMeshVolume(localVertices, localTets);
            var localRemainVolume = GeometryPredicates.FaceMeshVolume(localVertices, localRemaining);
            var diff = (localRemainVolume + localTetVolume) - localBoundaryVolume;

            if (Math.Abs(diff) > 1e-8)
            {
                continue;
            }

            if (GeometryPredicates.HasMeshIntersections(localVertices, localRemaining))
            {
                continue;
            }

            if (localRemaining.Count > 0 && depth < options.MaxDelaunayRecursionDepth)
            {
                var recurse = BuildRecursive(localVertices, localRemaining, options, depth + 1);
                localTets.AddRange(recurse);
                localRemaining = MeshTopology.GetRemainingFaces(localTets, localFaces);
            }

            foreach (var lt in localTets)
            {
                total.Add(new Tetrahedron(
                    globalVertexIds[lt.A],
                    globalVertexIds[lt.B],
                    globalVertexIds[lt.C],
                    globalVertexIds[lt.D]));
            }
        }

        return total;
    }

    private static List<Tetrahedron> BuildLocal(
        IReadOnlyList<Vector3d> localVertices,
        IReadOnlyList<Face> localFaces,
        Mesh2TetraOptions options)
    {
        var dverts = localVertices.Select((v, i) => new DVertex(i, v)).ToList();
        var triangulation = DelaunayTriangulation<DVertex, DefaultTriangulationCell<DVertex>>.Create(
            dverts,
            options.PlaneDistanceTolerance);

        var result = new List<Tetrahedron>();
        foreach (var cell in triangulation.Cells)
        {
            var ids = cell.Vertices.Select(v => v.Id).ToArray();
            var centroid = (localVertices[ids[0]] + localVertices[ids[1]] + localVertices[ids[2]] + localVertices[ids[3]]) / 4d;
            if (!GeometryPredicates.PointInsideClosedMesh(centroid, localVertices, localFaces))
            {
                continue;
            }

            var tet = new Tetrahedron(ids[0], ids[1], ids[2], ids[3]);
            var volume = Math.Abs(GeometryPredicates.SignedTetraVolume(
                localVertices[tet.A],
                localVertices[tet.B],
                localVertices[tet.C],
                localVertices[tet.D]));
            if (volume > options.Epsilon)
            {
                result.Add(tet);
            }
        }

        return result;
    }

    private sealed class DVertex(int id, Vector3d p) : IVertex
    {
        public int Id { get; } = id;
        public double[] Position { get; } = [p.X, p.Y, p.Z];
    }
}
