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

        // Drop numerically-degenerate triangles.
        result = result.Where(f => !IsDegenerateByArea(vertices[f.A], vertices[f.B], vertices[f.C], options.Epsilon)).ToList();

        // Resolve exact duplicate triangles (same 3 vertices, any winding) by pair-cancellation.
        result = RemoveDuplicateFacePairs(result);

        // Parity helper: attempt global orientation flip if enabled and needed.
        if (options.AutoFixFaceOrientation && GeometryPredicates.HasOrientationImbalance(result))
        {
            result = MeshTopology.FlipOrientation(result);
        }

        var hasIntersections = GeometryPredicates.HasMeshIntersections(vertices, result);
        if (hasIntersections && options.AutoResolveIntersections)
        {
            // Minimal deterministic fallback: remove all faces participating in detected intersection pairs.
            var filtered = RemoveIntersectingFaces(vertices, result);
            if (filtered.Count >= 4 && !GeometryPredicates.HasMeshIntersections(vertices, filtered))
            {
                result = filtered;
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

    private static bool IsDegenerateByArea(Vector3d a, Vector3d b, Vector3d c, double eps)
    {
        var area2 = Vector3d.Cross(b - a, c - a).Norm();
        return area2 <= eps;
    }

    private static List<Face> RemoveDuplicateFacePairs(IReadOnlyList<Face> faces)
    {
        var buckets = faces
            .Select((f, i) => (Face: f, Index: i, Key: MeshTopology.Canonical(f)))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.Select(v => v.Face).ToList());

        var result = new List<Face>();
        foreach (var kv in buckets)
        {
            var keep = kv.Value.Count % 2;
            if (keep == 1)
            {
                result.Add(kv.Value[0]);
            }
        }

        return result;
    }

    private static List<Face> RemoveIntersectingFaces(IReadOnlyList<Vector3d> vertices, IReadOnlyList<Face> faces)
    {
        var remove = new bool[faces.Count];
        for (var i = 0; i < faces.Count; i++)
        {
            for (var j = i + 1; j < faces.Count; j++)
            {
                var fi = faces[i];
                var fj = faces[j];
                if (MeshTopology.Canonical(fi) == MeshTopology.Canonical(fj)) continue;

                if (GeometryPredicates.TriangleTriangleIntersection(
                    vertices[fi.A], vertices[fi.B], vertices[fi.C],
                    vertices[fj.A], vertices[fj.B], vertices[fj.C],
                    ignoreCorners: true))
                {
                    remove[i] = true;
                    remove[j] = true;
                }
            }
        }

        var result = new List<Face>();
        for (var i = 0; i < faces.Count; i++)
        {
            if (!remove[i]) result.Add(faces[i]);
        }

        return result;
    }
}
