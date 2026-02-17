using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;
using MIConvexHull;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class DelaunayInside3D
{
    public static IReadOnlyList<Tetrahedron> Build(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> boundaryFaces, Mesh2TetraOptions options)
    {
        var delaunayVertices = vertices.Select((v, i) => new DVertex(i, v)).ToList();
        var triangulation = DelaunayTriangulation<DVertex, DefaultTriangulationCell<DVertex>>.Create(delaunayVertices);

        var result = new List<Tetrahedron>();
        foreach (var cell in triangulation.Cells)
        {
            var ids = cell.Vertices.Select(v => v.Id).ToArray();
            var centroid = (vertices[ids[0]] + vertices[ids[1]] + vertices[ids[2]] + vertices[ids[3]]) / 4d;

            if (!GeometryPredicates.PointInsideClosedMesh(centroid, vertices, boundaryFaces))
            {
                continue;
            }

            var tet = new Tetrahedron(ids[0], ids[1], ids[2], ids[3]);
            var volume = Math.Abs(GeometryPredicates.SignedTetraVolume(vertices[tet.A], vertices[tet.B], vertices[tet.C], vertices[tet.D]));
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
