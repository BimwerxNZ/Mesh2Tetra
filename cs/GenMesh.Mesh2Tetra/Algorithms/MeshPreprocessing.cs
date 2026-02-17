using GenMesh.Mesh2Tetra.Geometry;
using GenMesh.Mesh2Tetra.Models;

namespace GenMesh.Mesh2Tetra.Algorithms;

internal static class MeshPreprocessing
{
    public static List<Face> PreprocessBoundaryFaces(
        IReadOnlyList<Vector3d> vertices,
        IReadOnlyList<Face> faces,
        Mesh2TetraOptions options)
    {
        var result = faces.ToList();

        result = result.Where(f => !IsDegenerateByArea(vertices[f.A], vertices[f.B], vertices[f.C], options.Epsilon)).ToList();
        result = RemoveDuplicateFacePairs(result);

        if (options.AutoFixFaceOrientation && GeometryPredicates.HasOrientationImbalance(result))
        {
            result = MeshTopology.FlipOrientation(result);
        }

        if (options.AutoResolveIntersections)
        {
            result = SolveIntersectionsByLocalCollapse(vertices, result, options);

            // final conservative fallback
            if (GeometryPredicates.HasMeshIntersections(vertices, result))
            {
                var filtered = RemoveIntersectingFaces(vertices, result);
                if (filtered.Count >= 4 && !GeometryPredicates.HasMeshIntersections(vertices, filtered))
                {
                    result = filtered;
                }
            }
        }

        if (options.FailOnSelfIntersections && GeometryPredicates.HasMeshIntersections(vertices, result))
        {
            throw new InvalidOperationException(
                "Boundary mesh still has self-intersections after preprocessing. " +
                "Disable FailOnSelfIntersections to continue at your own risk.");
        }

        return result;
    }

    private static List<Face> SolveIntersectionsByLocalCollapse(
        IReadOnlyList<Vector3d> vertices,
        List<Face> faces,
        Mesh2TetraOptions options)
    {
        var current = faces;
        var previousCount = int.MaxValue;

        for (var iteration = 0; iteration < options.MaxSolveIntersectionIterations; iteration++)
        {
            var pairs = GeometryPredicates.FindIntersectingFacePairs(vertices, current);
            var intersectionCount = pairs.Count * 2;
            if (intersectionCount == 0)
            {
                return current;
            }

            if (intersectionCount >= previousCount)
            {
                break;
            }

            previousCount = intersectionCount;
            var involvedFaceIndices = pairs.SelectMany(p => new[] { p.I, p.J }).Distinct().ToArray();
            var involvedVertices = involvedFaceIndices
                .SelectMany(i => new[] { current[i].A, current[i].B, current[i].C })
                .Distinct()
                .ToArray();

            var improved = false;
            foreach (var vertexId in involvedVertices)
            {
                var localRows = current
                    .Select((f, idx) => (f, idx))
                    .Where(x => x.f.A == vertexId || x.f.B == vertexId || x.f.C == vertexId)
                    .Select(x => x.idx)
                    .ToList();

                if (localRows.Count == 0) continue;
                var localFaces = localRows.Select(i => current[i]).ToList();
                var neighbors = localFaces
                    .SelectMany(f => new[] { f.A, f.B, f.C })
                    .Where(v => v != vertexId)
                    .Distinct()
                    .ToArray();

                foreach (var neighbor in neighbors)
                {
                    var candidate = TryLocalCollapse(current, localRows, localFaces, vertexId, neighbor);
                    if (candidate.Count == 0) continue;

                    var candidateIntersections = GeometryPredicates.FindIntersectingFacePairs(vertices, candidate).Count * 2;
                    if (candidateIntersections < intersectionCount)
                    {
                        current = candidate;
                        improved = true;
                        break;
                    }
                }

                if (improved)
                {
                    break;
                }
            }

            if (!improved)
            {
                break;
            }
        }

        return current;
    }

    private static List<Face> TryLocalCollapse(
        List<Face> faces,
        List<int> localRows,
        List<Face> localFaces,
        int vertexId,
        int neighborId)
    {
        var localNew = localFaces
            .Select(f => ReplaceVertex(f, vertexId, neighborId))
            .Where(f => !IsDegenerate(f))
            .ToList();

        if (localNew.Count == 0)
        {
            return [];
        }

        var result = faces.ToList();

        foreach (var idx in localRows.OrderByDescending(v => v))
        {
            result.RemoveAt(idx);
        }

        foreach (var f in localNew)
        {
            var duplicate = result.FindIndex(x => MeshTopology.Canonical(x) == MeshTopology.Canonical(f));
            if (duplicate >= 0)
            {
                result.RemoveAt(duplicate);
            }
            else
            {
                result.Add(f);
            }
        }

        return result;
    }

    private static bool IsDegenerateByArea(Vector3d a, Vector3d b, Vector3d c, double eps)
    {
        var area2 = Vector3d.Cross(b - a, c - a).Norm();
        return area2 <= eps;
    }

    private static List<Face> RemoveDuplicateFacePairs(IReadOnlyList<Face> faces)
    {
        var buckets = faces
            .GroupBy(MeshTopology.Canonical)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<Face>();
        foreach (var kv in buckets)
        {
            if ((kv.Value.Count % 2) == 1)
            {
                result.Add(kv.Value[0]);
            }
        }

        return result;
    }

    private static List<Face> RemoveIntersectingFaces(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> faces)
    {
        var remove = new bool[faces.Count];
        var pairs = GeometryPredicates.FindIntersectingFacePairs(vertices, faces);
        foreach (var p in pairs)
        {
            remove[p.I] = true;
            remove[p.J] = true;
        }

        var result = new List<Face>();
        for (var i = 0; i < faces.Count; i++)
        {
            if (!remove[i]) result.Add(faces[i]);
        }

        return result;
    }

    private static Face ReplaceVertex(Face f, int from, int to)
        => new(f.A == from ? to : f.A, f.B == from ? to : f.B, f.C == from ? to : f.C);

    private static bool IsDegenerate(Face f) => f.A == f.B || f.B == f.C || f.A == f.C;
}
