using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class BoundaryCollapse3D
{
    public static IReadOnlyList<Tetrahedron> FillResidualVolume(
        IReadOnlyList<Vector3d> vertices,
        IReadOnlyList<Face> residualFaces,
        IReadOnlyList<Tetrahedron> existing,
        Mesh2TetraOptions options)
    {
        var boundary = residualFaces.ToList();
        var tetrahedra = existing.ToList();
        var originalVolume = GeometryPredicates.FaceMeshVolume(vertices, boundary) + GeometryPredicates.TetraMeshVolume(vertices, tetrahedra);
        var rng = new Random(1234);

        var retry = 0;
        var mode = 0;
        while (boundary.Count > 0)
        {
            var countBefore = tetrahedra.Count;
            var collapsed = TryCollapseEdge(vertices, boundary, tetrahedra, originalVolume, mode, rng, options);
            if (!collapsed)
            {
                mode = 1;
                retry++;
                RetryRemoveTetrahedrons(boundary, tetrahedra);
                if (retry % 5 == 0) RetryRemoveTetrahedrons(boundary, tetrahedra);
                if (retry % 10 == 0) RetryRemoveTetrahedrons(boundary, tetrahedra);
                if (retry > 25)
                {
                    throw new InvalidOperationException("Boundary collapse failed after retries.");
                }
            }

            if (countBefore == tetrahedra.Count && !collapsed && boundary.Count == 0)
            {
                break;
            }
        }

        var finalVolume = GeometryPredicates.TetraMeshVolume(vertices, tetrahedra);
        if (Math.Abs(finalVolume - originalVolume) > 1e-6)
        {
            throw new InvalidOperationException($"Boundary collapse volume mismatch. expected={originalVolume}, actual={finalVolume}");
        }

        return tetrahedra;
    }

    private static bool TryCollapseEdge(
        IReadOnlyList<Vector3d> vertices,
        List<Face> boundary,
        List<Tetrahedron> tetrahedra,
        double originalVolume,
        int mode,
        Random rng,
        Mesh2TetraOptions options)
    {
        var vertexIds = boundary.SelectMany(f => new[] { f.A, f.B, f.C }).Distinct().ToList();
        if (mode == 1)
        {
            vertexIds = vertexIds.OrderBy(_ => rng.NextDouble()).ToList();
        }

        foreach (var vertexId in vertexIds)
        {
            var localRows = boundary.Select((f, idx) => (f, idx)).Where(x => HasVertex(x.f, vertexId)).ToList();
            var localFaces = localRows.Select(x => x.f).ToList();
            var localNeighbors = localFaces.SelectMany(f => new[] { f.A, f.B, f.C }).Distinct().Where(v => v != vertexId).ToList();

            foreach (var localVertex in localNeighbors)
            {
                var localNew = localFaces
                    .Select(f => ReplaceVertex(f, vertexId, localVertex))
                    .Where(f => !IsDegenerate(f))
                    .ToList();

                if (localNew.Count == 0)
                {
                    continue;
                }

                var b2 = boundary.ToList();
                var t2 = tetrahedra.ToList();
                Process(b2, t2, localRows.Select(x => x.idx).ToList(), localFaces, localNew, vertexId);

                if (HasVolumeError(vertices, b2, t2, originalVolume)) continue;
                if (GeometryPredicates.HasOrientationImbalance(b2)) continue;
                if (!GeometryPredicates.CheckMoveInside3D(vertices, localNew, vertexId)) continue;

                boundary.Clear();
                boundary.AddRange(b2);
                tetrahedra.Clear();
                tetrahedra.AddRange(t2);
                return true;
            }
        }

        return false;
    }

    private static void Process(List<Face> boundary, List<Tetrahedron> tetrahedra, List<int> localRows, List<Face> localFaces, List<Face> localNew, int vertexId)
    {
        foreach (var idx in localRows.OrderByDescending(v => v))
        {
            boundary.RemoveAt(idx);
        }

        foreach (var f in localNew)
        {
            var existingIdx = boundary.FindIndex(x => MeshTopology.Canonical(x) == MeshTopology.Canonical(f));
            if (existingIdx >= 0)
            {
                boundary.RemoveAt(existingIdx);
            }
            else
            {
                boundary.Add(f);
            }

            tetrahedra.Add(new Tetrahedron(f.A, f.B, f.C, vertexId));
        }
    }

    private static void RetryRemoveTetrahedrons(List<Face> boundary, List<Tetrahedron> tetrahedra)
    {
        if (boundary.Count == 0 || tetrahedra.Count == 0) return;

        var removeTetra = new HashSet<int>();
        var removeFace = new HashSet<int>();

        for (var i = 0; i < boundary.Count; i++)
        {
            var f = boundary[i];
            var j = tetrahedra.FindIndex(t => MeshTopology.ContainsAllVertices(t, f));
            if (j >= 0)
            {
                removeTetra.Add(j);
                removeFace.Add(i);
            }
        }

        var removedBoundaryFaces = removeFace.Select(i => boundary[i]).ToList();
        var newFaces = new List<Face>();
        foreach (var tIndex in removeTetra)
        {
            newFaces.AddRange(MeshTopology.GetTetFaces(tetrahedra[tIndex]));
        }

        var removedBoundaryCanon = removedBoundaryFaces.Select(MeshTopology.Canonical).ToHashSet();
        var counts = newFaces.GroupBy(MeshTopology.Canonical).ToDictionary(g => g.Key, g => g.Count());
        var outerFaces = newFaces.Where(f => !removedBoundaryCanon.Contains(MeshTopology.Canonical(f)) && counts[MeshTopology.Canonical(f)] == 1).ToList();

        foreach (var i in removeFace.OrderByDescending(v => v)) boundary.RemoveAt(i);
        foreach (var i in removeTetra.OrderByDescending(v => v)) tetrahedra.RemoveAt(i);
        boundary.AddRange(outerFaces);
    }

    private static bool HasVolumeError(IReadOnlyList<Vector3d> vertices, List<Face> boundary, List<Tetrahedron> tetrahedra, double originalVolume)
    {
        var v = GeometryPredicates.FaceMeshVolume(vertices, boundary) + GeometryPredicates.TetraMeshVolume(vertices, tetrahedra);
        return Math.Abs(v - originalVolume) > 1e-7;
    }

    private static bool HasVertex(Face f, int id) => f.A == id || f.B == id || f.C == id;

    private static Face ReplaceVertex(Face f, int from, int to)
        => new(f.A == from ? to : f.A, f.B == from ? to : f.B, f.C == from ? to : f.C);

    private static bool IsDegenerate(Face f) => f.A == f.B || f.B == f.C || f.A == f.C;
}
